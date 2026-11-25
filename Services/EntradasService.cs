using lsfinan.Data;
using lsfinan.Exceptions;
using lsfinan.Interfaces;
using lsfinan.Models;
using Microsoft.EntityFrameworkCore;

namespace lsfinan.Services
{
    public class EntradasService : IEntradaService
    {
        private readonly AppDbContext _context;
        private readonly IContaService _contaService;

        public EntradasService(AppDbContext context, IContaService contaService)
        {
            _context = context;
            _contaService = contaService;
        }

        public async Task<List<Entrada>> ListarTodasEntradas(Usuario usuario)
        {
            return await _context.Entradas
                .Where(x => x.UsuarioId == usuario.Id)
                .OrderByDescending(x => x.DataEntrada)
                .ToListAsync();
        }

        public async Task<List<Entrada>> ListarEntradas(Usuario usuario)
        {
            var hoje = DateTime.Today;
            var amanha = hoje.AddDays(1);

            return await _context.Entradas
                .Where(e => e.UsuarioId == usuario.Id
                         && e.DataEntrada >= hoje
                         && e.DataEntrada < amanha)
                .ToListAsync();
        }

        public async Task<Entrada> CadastrarEntrada(EntradaViewModel entrada, Usuario usuario)
        {
            var valor = RegrasFinanceiras.Arredondar(entrada.ValorEntrada);

            if (valor <= 0m)
            {
                throw new RegraDeNegocioException("Informe um valor maior que zero.");
            }

            await ValidarCategoriaEConta(entrada, usuario, null);

            var descricao = entrada.DescricaoEntrada.Trim();

            var novaEntrada = new Entrada
            {
                NomeEntrada = descricao,
                DescricaoEntrada = descricao,
                ValorEntrada = valor,
                DataEntrada = DateTime.Now,
                CategoriaId = entrada.CategoriaId,
                ContaId = entrada.ContaId,
                UsuarioId = usuario.Id
            };

            _context.Entradas.Add(novaEntrada);
            await _context.SaveChangesAsync();

            return novaEntrada;
        }

        public async Task<EntradaViewModel> ObterParaEdicao(int id, Usuario usuario)
        {
            var entrada = await BuscarEntrada(id, usuario);

            // categoria/conta atuais continuam na lista mesmo se foram inativadas depois
            var categorias = await _context.Categorias
                .Where(x => x.UsuarioId == usuario.Id && (x.Ativa || x.Id == entrada.CategoriaId))
                .OrderBy(x => x.NomeCategoria)
                .ToListAsync();

            var contas = await _context.Contas
                .Where(x => x.UsuarioId == usuario.Id && (x.Ativa || x.Id == entrada.ContaId))
                .OrderBy(x => x.NomeConta)
                .ToListAsync();

            return new EntradaViewModel
            {
                Id = entrada.Id,
                NomeEntrada = entrada.NomeEntrada,
                DescricaoEntrada = entrada.DescricaoEntrada,
                ValorEntrada = entrada.ValorEntrada,
                CategoriaId = entrada.CategoriaId,
                ContaId = entrada.ContaId,
                Categorias = categorias,
                Contas = contas
            };
        }

        public async Task EditarEntrada(int id, EntradaViewModel viewModel, Usuario usuario)
        {
            var entrada = await BuscarEntrada(id, usuario);

            var novoValor = RegrasFinanceiras.Arredondar(viewModel.ValorEntrada);

            if (novoValor <= 0m)
            {
                throw new RegraDeNegocioException("Informe um valor maior que zero.");
            }

            await ValidarCategoriaEConta(viewModel, usuario, entrada);

            // Regra: editar uma entrada não pode deixar nenhuma conta com saldo negativo.
            var saldoContaAntiga = await _contaService.ObterSaldoAtual(entrada.ContaId, usuario.Id);

            if (entrada.ContaId == viewModel.ContaId)
            {
                var saldoDepois = saldoContaAntiga - entrada.ValorEntrada + novoValor;

                if (saldoDepois < 0m && saldoDepois < saldoContaAntiga)
                {
                    throw new RegraDeNegocioException(
                        $"Com esse valor o saldo da conta ficaria negativo ({RegrasFinanceiras.Moeda(saldoDepois)}), " +
                        "porque já existem saídas contando com esse dinheiro.");
                }
            }
            else
            {
                var saldoSemEla = saldoContaAntiga - entrada.ValorEntrada;

                if (saldoSemEla < 0m)
                {
                    throw new RegraDeNegocioException(
                        "Não dá para mover essa entrada: a conta de origem ficaria com saldo " +
                        $"negativo ({RegrasFinanceiras.Moeda(saldoSemEla)}).");
                }
            }

            var descricao = viewModel.DescricaoEntrada.Trim();

            entrada.NomeEntrada = descricao;
            entrada.DescricaoEntrada = descricao;
            entrada.ValorEntrada = novoValor;
            entrada.CategoriaId = viewModel.CategoriaId;
            entrada.ContaId = viewModel.ContaId;

            await _context.SaveChangesAsync();
        }

        public async Task ExcluirEntrada(int id, Usuario usuario)
        {
            var entrada = await BuscarEntrada(id, usuario);

            var saldo = await _contaService.ObterSaldoAtual(entrada.ContaId, usuario.Id);
            var saldoDepois = saldo - entrada.ValorEntrada;

            if (saldoDepois < 0m)
            {
                throw new RegraDeNegocioException(
                    "Não dá para excluir essa entrada: a conta ficaria com saldo negativo " +
                    $"({RegrasFinanceiras.Moeda(saldoDepois)}) porque já existem saídas " +
                    "usando esse dinheiro.");
            }

            _context.Entradas.Remove(entrada);
            await _context.SaveChangesAsync();
        }

        private async Task<Entrada> BuscarEntrada(int id, Usuario usuario)
        {
            var entrada = await _context.Entradas
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuario.Id);

            if (entrada == null)
            {
                throw new RegraDeNegocioException("Entrada não encontrada.");
            }

            return entrada;
        }

        /// <summary>
        /// Categoria e conta precisam ser do usuário e estar ativas
        /// (na edição, manter a mesma já inativada ainda é permitido).
        /// </summary>
        private async Task ValidarCategoriaEConta(
            EntradaViewModel viewModel,
            Usuario usuario,
            Entrada? original)
        {
            var categoria = await _context.Categorias.FirstOrDefaultAsync(x =>
                x.Id == viewModel.CategoriaId && x.UsuarioId == usuario.Id);

            if (categoria == null)
            {
                throw new RegraDeNegocioException("Categoria não encontrada.");
            }

            if (!categoria.Ativa && original?.CategoriaId != categoria.Id)
            {
                throw new RegraDeNegocioException(
                    "A categoria selecionada está inativa. Escolha outra categoria.");
            }

            var conta = await _context.Contas.FirstOrDefaultAsync(x =>
                x.Id == viewModel.ContaId && x.UsuarioId == usuario.Id);

            if (conta == null)
            {
                throw new RegraDeNegocioException("Conta não encontrada.");
            }

            if (!conta.Ativa && original?.ContaId != conta.Id)
            {
                throw new RegraDeNegocioException(
                    "A conta selecionada está inativa. Escolha outra conta.");
            }
        }
    }
}
