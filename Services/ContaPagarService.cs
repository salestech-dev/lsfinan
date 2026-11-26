using lsfinan.Data;
using lsfinan.Enum;
using lsfinan.Exceptions;
using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace lsfinan.Services
{
    public class ContaPagarService : IContaPagarService
    {
        private readonly AppDbContext _context;
        private readonly ISaidaService _saidaService;
        private readonly IContaService _contaService;

        public ContaPagarService(
            AppDbContext context,
            ISaidaService saidaService,
            IContaService contaService)
        {
            _context = context;
            _saidaService = saidaService;
            _contaService = contaService;
        }

        // ------------------------------------------------------------------
        // LISTAGEM E ALERTAS
        // ------------------------------------------------------------------

        public async Task<ContaPagarListagemViewModel> Listar(Usuario usuario, string filtro)
        {
            var hoje = DateTime.Today;

            var categorias = await _context.Categorias
                .Where(x => x.UsuarioId == usuario.Id)
                .ToListAsync();

            var resultado = new ContaPagarListagemViewModel
            {
                Filtro = filtro == "pagas" ? "pagas" : "pendentes",
                JanelaAlertaDias = RegrasFinanceiras.JanelaAlertaDias
            };

            if (resultado.Filtro == "pagas")
            {
                var pagas = await _context.ContasPagar
                    .Where(x => x.UsuarioId == usuario.Id && x.Pago)
                    .OrderByDescending(x => x.DataPagamento)
                    .Take(100)
                    .ToListAsync();

                var ids = pagas.Select(p => p.Id).ToList();

                var saidas = await _context.Saidas
                    .Where(x => x.UsuarioId == usuario.Id &&
                                x.ContaPagarId != null &&
                                ids.Contains(x.ContaPagarId.Value))
                    .ToListAsync();

                var contas = await _context.Contas
                    .Where(x => x.UsuarioId == usuario.Id)
                    .ToListAsync();

                foreach (var conta in pagas)
                {
                    var item = Mapear(conta, categorias, hoje);

                    item.ContasPagamento = string.Join(" + ", saidas
                        .Where(s => s.ContaPagarId == conta.Id)
                        .Select(s => contas.FirstOrDefault(c => c.Id == s.ContaId)?.NomeConta ?? "Conta removida")
                        .Distinct());

                    resultado.Pagas.Add(item);
                }

                return resultado;
            }

            var pendentes = await _context.ContasPagar
                .Where(x => x.UsuarioId == usuario.Id && !x.Pago)
                .OrderBy(x => x.DataVencimento)
                .ThenBy(x => x.NomeContaPagar)
                .ToListAsync();

            foreach (var conta in pendentes)
            {
                var item = Mapear(conta, categorias, hoje);

                switch (item.Status)
                {
                    case EnumStatusContaPagar.Atrasada:
                        resultado.Atrasadas.Add(item);
                        break;
                    case EnumStatusContaPagar.VenceHoje:
                        resultado.VencemHoje.Add(item);
                        break;
                    case EnumStatusContaPagar.VenceEmBreve:
                        resultado.ProximosDias.Add(item);
                        break;
                    default:
                        resultado.Futuras.Add(item);
                        break;
                }
            }

            return resultado;
        }

        public async Task<AlertasContasPagarViewModel> ObterAlertas(int usuarioId)
        {
            var hoje = DateTime.UtcNow.Date;
            var limiteExclusivo = hoje.AddDays(RegrasFinanceiras.JanelaAlertaDias + 1);

            var abertas = await _context.ContasPagar
                .Where(x => x.UsuarioId == usuarioId && !x.Pago && x.DataVencimento < limiteExclusivo)
                .OrderBy(x => x.DataVencimento)
                .ThenBy(x => x.NomeContaPagar)
                .ToListAsync();

            var alertas = new AlertasContasPagarViewModel
            {
                JanelaDias = RegrasFinanceiras.JanelaAlertaDias
            };

            foreach (var conta in abertas)
            {
                var item = Mapear(conta, new List<Categoria>(), hoje);

                switch (item.Status)
                {
                    case EnumStatusContaPagar.Atrasada:
                        alertas.QtdAtrasadas++;
                        alertas.TotalAtrasadas += item.Valor;
                        break;
                    case EnumStatusContaPagar.VenceHoje:
                        alertas.QtdHoje++;
                        alertas.TotalHoje += item.Valor;
                        break;
                    default:
                        alertas.QtdProximos++;
                        alertas.TotalProximos += item.Valor;
                        break;
                }

                alertas.Itens.Add(item);
            }

            alertas.Itens = alertas.Itens.Take(6).ToList();

            return alertas;
        }

        // ------------------------------------------------------------------
        // CADASTRO, EDIÇÃO E EXCLUSÃO
        // ------------------------------------------------------------------

        public async Task<ContaPagarViewModel> NovoFormulario(Usuario usuario)
        {
            var viewModel = new ContaPagarViewModel
            {
                DataVencimento = DateTime.Today.AddDays(7)
            };

            await PreencherCategorias(viewModel, usuario);

            return viewModel;
        }

        public async Task PreencherCategorias(ContaPagarViewModel viewModel, Usuario usuario)
        {
            // a categoria atual continua na lista mesmo se foi inativada depois
            viewModel.Categorias = await _context.Categorias
                .Where(x => x.UsuarioId == usuario.Id &&
                            (x.Ativa || x.Id == viewModel.CategoriaId))
                .OrderBy(x => x.NomeCategoria)
                .ToListAsync();
        }

        public async Task<ContaPagarViewModel> ObterParaEdicao(int id, Usuario usuario)
        {
            var conta = await BuscarConta(id, usuario);

            if (conta.Pago)
            {
                throw new RegraDeNegocioException(
                    "Essa conta já foi paga. Desfaça o pagamento para poder editá-la.");
            }

            var viewModel = new ContaPagarViewModel
            {
                Id = conta.Id,
                NomeContaPagar = conta.NomeContaPagar,
                DescricaoContaPagar = conta.DescricaoContaPagar,
                ValorContaPagar = conta.ValorContaPagar,
                DataVencimento = conta.DataVencimento.Date,
                CategoriaId = conta.CategoriaId
            };

            await PreencherCategorias(viewModel, usuario);

            return viewModel;
        }

        public async Task<ContaPagar> Cadastrar(ContaPagarViewModel viewModel, Usuario usuario)
        {
            await ValidarFormulario(viewModel, usuario, null);

            var nova = new ContaPagar
            {
                NomeContaPagar = viewModel.NomeContaPagar.Trim(),
                DescricaoContaPagar = viewModel.DescricaoContaPagar?.Trim() ?? string.Empty,
                ValorContaPagar = RegrasFinanceiras.Arredondar(viewModel.ValorContaPagar),
                DataVencimento = viewModel.DataVencimento!.Value.Date,
                CategoriaId = viewModel.CategoriaId,
                Pago = false,
                DataCadastro = DateTime.UtcNow,
                UsuarioId = usuario.Id
            };

            _context.ContasPagar.Add(nova);
            await _context.SaveChangesAsync();

            return nova;
        }

        public async Task Editar(int id, ContaPagarViewModel viewModel, Usuario usuario)
        {
            var conta = await BuscarConta(id, usuario);

            if (conta.Pago)
            {
                throw new RegraDeNegocioException(
                    "Essa conta já foi paga. Desfaça o pagamento para poder editá-la.");
            }

            await ValidarFormulario(viewModel, usuario, conta);

            conta.NomeContaPagar = viewModel.NomeContaPagar.Trim();
            conta.DescricaoContaPagar = viewModel.DescricaoContaPagar?.Trim() ?? string.Empty;
            conta.ValorContaPagar = RegrasFinanceiras.Arredondar(viewModel.ValorContaPagar);
            conta.DataVencimento = viewModel.DataVencimento!.Value.Date;
            conta.CategoriaId = viewModel.CategoriaId;

            await _context.SaveChangesAsync();
        }

        public async Task Excluir(int id, Usuario usuario)
        {
            var conta = await BuscarConta(id, usuario);

            if (conta.Pago)
            {
                throw new RegraDeNegocioException(
                    "Essa conta já foi paga. Desfaça o pagamento antes de excluí-la.");
            }

            _context.ContasPagar.Remove(conta);
            await _context.SaveChangesAsync();
        }

        // ------------------------------------------------------------------
        // PAGAMENTO
        // ------------------------------------------------------------------

        public async Task<PagarContaPagarViewModel> ObterParaPagamento(int id, Usuario usuario)
        {
            var conta = await BuscarConta(id, usuario);

            if (conta.Pago)
            {
                throw new RegraDeNegocioException("Essa conta já foi paga.");
            }

            var categorias = await _context.Categorias
                .Where(x => x.UsuarioId == usuario.Id)
                .ToListAsync();

            var contasAtivas = await _contaService.ListarContasAtivasComSaldo(usuario);

            var viewModel = new PagarContaPagarViewModel
            {
                Id = conta.Id,
                Conta = Mapear(conta, categorias, DateTime.UtcNow),
                ContasAtivas = contasAtivas
            };

            // Só uma conta ativa e ela cobre o valor: já deixa preenchido.
            if (contasAtivas.Count == 1 &&
                RegrasFinanceiras.Disponivel(contasAtivas[0].SaldoAtual) >= conta.ValorContaPagar)
            {
                viewModel.Alocacoes = RegrasFinanceiras.SugerirDivisao(
                    conta.ValorContaPagar, contasAtivas[0].Id, contasAtivas);
            }

            return viewModel;
        }

        /// <summary>
        /// Paga a conta com uma conta bancária ou dividindo entre várias.
        /// Gera as saídas e marca como paga na mesma gravação: ou tudo, ou nada.
        /// </summary>
        public async Task Pagar(int id, List<AlocacaoPagamentoViewModel> alocacoes, Usuario usuario)
        {
            var conta = await BuscarConta(id, usuario);

            if (conta.Pago)
            {
                throw new RegraDeNegocioException("Essa conta já foi paga.");
            }

            await _saidaService.AdicionarSaidasAsync(usuario, new NovaSaidaDados
            {
                Nome = conta.NomeContaPagar,
                Descricao = MontarDescricaoDoPagamento(conta),
                Valor = conta.ValorContaPagar,
                CategoriaId = conta.CategoriaId,
                Alocacoes = alocacoes ?? new List<AlocacaoPagamentoViewModel>(),
                ContaPagarId = conta.Id,
                ExigirCategoriaAtiva = false
            });

            conta.Pago = true;
            conta.DataPagamento = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DesfazerPagamento(int id, Usuario usuario)
        {
            var conta = await BuscarConta(id, usuario);

            if (!conta.Pago)
            {
                throw new RegraDeNegocioException("Essa conta ainda não foi paga.");
            }

            var saidas = await _context.Saidas
                .Where(x => x.UsuarioId == usuario.Id && x.ContaPagarId == conta.Id)
                .ToListAsync();

            _context.Saidas.RemoveRange(saidas);

            conta.Pago = false;
            conta.DataPagamento = null;

            await _context.SaveChangesAsync();
        }

        // ------------------------------------------------------------------
        // AUXILIARES
        // ------------------------------------------------------------------

        private async Task<ContaPagar> BuscarConta(int id, Usuario usuario)
        {
            var conta = await _context.ContasPagar
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuario.Id);

            if (conta == null)
            {
                throw new RegraDeNegocioException("Conta a pagar não encontrada.");
            }

            return conta;
        }

        private async Task ValidarFormulario(
            ContaPagarViewModel viewModel,
            Usuario usuario,
            ContaPagar? original)
        {
            if (RegrasFinanceiras.Arredondar(viewModel.ValorContaPagar) <= 0m)
            {
                throw new RegraDeNegocioException("Informe um valor maior que zero.");
            }

            if (viewModel.DataVencimento == null)
            {
                throw new RegraDeNegocioException("Informe a data de vencimento.");
            }

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
        }

        private static string MontarDescricaoDoPagamento(ContaPagar conta)
        {
            var texto = $"Conta a pagar (venc. {conta.DataVencimento:dd/MM/yyyy})";

            return string.IsNullOrWhiteSpace(conta.DescricaoContaPagar)
                ? texto
                : $"{conta.DescricaoContaPagar} · {texto}";
        }

        private static ContaPagarItemViewModel Mapear(
            ContaPagar conta,
            List<Categoria> categorias,
            DateTime hoje)
        {
            var dias = RegrasFinanceiras.DiasParaVencer(conta.DataVencimento, hoje);

            return new ContaPagarItemViewModel
            {
                Id = conta.Id,
                Nome = conta.NomeContaPagar,
                Descricao = conta.DescricaoContaPagar,
                Valor = conta.ValorContaPagar,
                DataVencimento = conta.DataVencimento.Date,
                NomeCategoria = categorias
                    .FirstOrDefault(c => c.Id == conta.CategoriaId)?.NomeCategoria ?? string.Empty,
                Pago = conta.Pago,
                DataPagamento = conta.DataPagamento,
                DiasParaVencer = dias,
                Status = RegrasFinanceiras.ClassificarStatus(conta.Pago, conta.DataVencimento, hoje),
                TextoVencimento = RegrasFinanceiras.TextoVencimento(conta.Pago, dias)
            };
        }
    }
}
