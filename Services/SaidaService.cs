using lsfinan.Data;
using lsfinan.Exceptions;
using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace lsfinan.Services
{
    public class SaidaService : ISaidaService
    {
        private readonly AppDbContext _context;
        private readonly IContaService _contaService;

        public SaidaService(AppDbContext context, IContaService contaService)
        {
            _context = context;
            _contaService = contaService;
        }

        // ------------------------------------------------------------------
        // CADASTRO
        // ------------------------------------------------------------------

        public async Task<List<Saida>> CadastrarSaida(
            Usuario usuario,
            SaidaViewModel saidaViewModel)
        {
            var saidas = await AdicionarSaidasAsync(usuario, new NovaSaidaDados
            {
                Nome = saidaViewModel.NomeSaida,
                Descricao = saidaViewModel.DescricaoSaida,
                Valor = saidaViewModel.ValorSaida,
                CategoriaId = saidaViewModel.CategoriaId,
                ContaId = saidaViewModel.ContaId,
                Alocacoes = saidaViewModel.Alocacoes
            });

            await _context.SaveChangesAsync();

            return saidas;
        }

        /// <summary>
        /// Regra central das saídas:
        ///  - o valor precisa ser maior que zero;
        ///  - categoria e conta têm que ser do usuário e estar ativas;
        ///  - a conta NUNCA fica negativa: se não houver saldo, lança
        ///    SaldoInsuficienteException para a tela pedir outra conta ou
        ///    a divisão do valor entre duas ou mais contas.
        /// Não salva: quem chama decide quando gravar.
        /// </summary>
        public async Task<List<Saida>> AdicionarSaidasAsync(Usuario usuario, NovaSaidaDados dados)
        {
            var valor = RegrasFinanceiras.Arredondar(dados.Valor);

            if (valor <= 0m)
            {
                throw new RegraDeNegocioException("Informe um valor maior que zero.");
            }

            var categoria = await _context.Categorias.FirstOrDefaultAsync(x =>
                x.Id == dados.CategoriaId && x.UsuarioId == usuario.Id);

            if (categoria == null)
            {
                throw new RegraDeNegocioException("Categoria não encontrada.");
            }

            if (dados.ExigirCategoriaAtiva && !categoria.Ativa)
            {
                throw new RegraDeNegocioException(
                    "A categoria selecionada está inativa. Escolha outra categoria.");
            }

            var contasAtivas = await _contaService.ListarContasAtivasComSaldo(usuario);
            var contasPorId = contasAtivas.ToDictionary(c => c.Id);

            var alocacoes = RegrasFinanceiras.NormalizarAlocacoes(dados.Alocacoes);

            if (alocacoes.Count == 0)
            {
                // Caminho simples: uma conta só, escolhida no formulário.
                if (dados.ContaId <= 0)
                {
                    throw new RegraDeNegocioException(
                        "Selecione a conta de onde o valor vai sair.");
                }

                if (!contasPorId.TryGetValue(dados.ContaId, out var conta))
                {
                    throw new RegraDeNegocioException(
                        "A conta selecionada não existe ou está inativa.");
                }

                if (valor > RegrasFinanceiras.Disponivel(conta.SaldoAtual))
                {
                    throw new SaldoInsuficienteException(
                        conta.Id,
                        conta.NomeConta,
                        conta.SaldoAtual,
                        valor,
                        contasAtivas,
                        RegrasFinanceiras.SugerirDivisao(valor, conta.Id, contasAtivas));
                }

                alocacoes.Add(new AlocacaoPagamentoViewModel { ContaId = conta.Id, Valor = valor });
            }
            else
            {
                RegrasFinanceiras.ValidarAlocacoes(valor, alocacoes, contasPorId);
            }

            var agora = DateTime.UtcNow;
            var criadas = new List<Saida>();
            var dividida = alocacoes.Count > 1;
            var descricaoBase = dados.Descricao?.Trim() ?? string.Empty;

            for (var i = 0; i < alocacoes.Count; i++)
            {
                var descricao = descricaoBase;

                if (dividida)
                {
                    var parte = $"Pagamento dividido entre contas ({i + 1}/{alocacoes.Count})";
                    descricao = string.IsNullOrEmpty(descricao) ? parte : $"{descricao} · {parte}";
                }

                criadas.Add(new Saida
                {
                    NomeSaida = dados.Nome.Trim(),
                    ValorSaida = alocacoes[i].Valor,
                    DescricaoSaida = descricao,
                    DataSaida = agora,
                    Pago = true,
                    DataPagamento = agora,
                    UsuarioId = usuario.Id,
                    ContaId = alocacoes[i].ContaId,
                    CategoriaId = categoria.Id,
                    ContaPagarId = dados.ContaPagarId
                });
            }

            await _context.Saidas.AddRangeAsync(criadas);

            return criadas;
        }

        // ------------------------------------------------------------------
        // EDIÇÃO E EXCLUSÃO
        // ------------------------------------------------------------------

        public async Task<SaidaViewModel> ObterParaEdicao(int id, Usuario usuario)
        {
            var saida = await BuscarSaidaEditavel(id, usuario);

            var categorias = await _context.Categorias
                .Where(x => x.UsuarioId == usuario.Id && (x.Ativa || x.Id == saida.CategoriaId))
                .OrderBy(x => x.NomeCategoria)
                .ToListAsync();

            var contas = await _context.Contas
                .Where(x => x.UsuarioId == usuario.Id && (x.Ativa || x.Id == saida.ContaId))
                .OrderBy(x => x.NomeConta)
                .ToListAsync();

            return new SaidaViewModel
            {
                Id = saida.Id,
                NomeSaida = saida.NomeSaida,
                ValorSaida = saida.ValorSaida,
                DescricaoSaida = saida.DescricaoSaida,
                CategoriaId = saida.CategoriaId,
                ContaId = saida.ContaId,
                Categorias = categorias,
                Contas = contas
            };
        }

        public async Task EditarSaida(int id, SaidaViewModel viewModel, Usuario usuario)
        {
            var saida = await BuscarSaidaEditavel(id, usuario);

            var novoValor = RegrasFinanceiras.Arredondar(viewModel.ValorSaida);

            if (novoValor <= 0m)
            {
                throw new RegraDeNegocioException("Informe um valor maior que zero.");
            }

            var categoria = await _context.Categorias.FirstOrDefaultAsync(x =>
                x.Id == viewModel.CategoriaId && x.UsuarioId == usuario.Id);

            if (categoria == null)
            {
                throw new RegraDeNegocioException("Categoria não encontrada.");
            }

            if (!categoria.Ativa && categoria.Id != saida.CategoriaId)
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

            if (!conta.Ativa && conta.Id != saida.ContaId)
            {
                throw new RegraDeNegocioException(
                    "A conta selecionada está inativa. Escolha outra conta.");
            }

            // Saldo da conta de destino sem contar esta saída (ela está sendo refeita).
            var saldo = await _contaService.ObterSaldoAtual(conta.Id, usuario.Id);

            if (saida.ContaId == conta.Id)
            {
                saldo += saida.ValorSaida;
            }

            if (novoValor > RegrasFinanceiras.Disponivel(saldo))
            {
                throw new RegraDeNegocioException(
                    $"A conta \"{conta.NomeConta}\" tem apenas " +
                    $"{RegrasFinanceiras.Moeda(RegrasFinanceiras.Disponivel(saldo))} " +
                    "disponível para esta saída. Escolha outra conta ou reduza o valor.");
            }

            saida.NomeSaida = viewModel.NomeSaida.Trim();
            saida.ValorSaida = novoValor;
            saida.DescricaoSaida = viewModel.DescricaoSaida?.Trim() ?? string.Empty;
            saida.CategoriaId = categoria.Id;
            saida.ContaId = conta.Id;

            await _context.SaveChangesAsync();
        }

        public async Task ExcluirSaida(int id, Usuario usuario)
        {
            var saida = await BuscarSaidaEditavel(id, usuario);

            // Excluir uma saída devolve o dinheiro para a conta: sempre permitido.
            _context.Saidas.Remove(saida);
            await _context.SaveChangesAsync();
        }

        private async Task<Saida> BuscarSaidaEditavel(int id, Usuario usuario)
        {
            var saida = await _context.Saidas
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuario.Id);

            if (saida == null)
            {
                throw new RegraDeNegocioException("Saída não encontrada.");
            }

            if (saida.ContaPagarId != null)
            {
                throw new RegraDeNegocioException(
                    "Essa saída foi gerada pelo pagamento de uma conta a pagar. " +
                    "Para alterá-la, desfaça o pagamento em Contas a pagar.");
            }

            return saida;
        }

        // ------------------------------------------------------------------
        // LISTAGENS
        // ------------------------------------------------------------------

        public async Task<List<Saida>> ListarSaida(Usuario usuario)
        {
            var saidas = await _context.Saidas
                .Where(x => x.UsuarioId == usuario.Id)
                .ToListAsync();

            await PreencherDadosDeExibicao(usuario, saidas);

            return saidas;
        }

        public async Task<List<Saida>> ListarSaidasDeHoje(Usuario usuario)
        {
            var hoje = DateTime.UtcNow.Date ;
            var amanha = hoje.AddDays(1);

            var saidas = await _context.Saidas
                .Where(x =>
                    x.UsuarioId == usuario.Id &&
                    x.DataSaida >= hoje &&
                    x.DataSaida < amanha)
                .OrderByDescending(x => x.DataSaida)
                .ToListAsync();

            await PreencherDadosDeExibicao(usuario, saidas);

            return saidas;
        }

        public async Task<TodasSaidasViewModel> ListarTodasSaidas(
            Usuario usuario,
            int mes,
            int ano,
            int pagina = 1)
        {
            const int tamanhoPagina = 10;

            if (pagina < 1)
            {
                pagina = 1;
            }

            var inicioMes = new DateTime(ano, mes, 1);
            var inicioProximoMes = inicioMes.AddMonths(1);

            var query = _context.Saidas
                .Where(x =>
                    x.UsuarioId == usuario.Id &&
                    x.DataSaida >= inicioMes &&
                    x.DataSaida < inicioProximoMes);

            var totalRegistros = await query.CountAsync();

            var totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)tamanhoPagina
            );

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            var saidas = await query
                .OrderByDescending(x => x.DataSaida)
                .ThenByDescending(x => x.Id)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            await PreencherDadosDeExibicao(usuario, saidas);

            return new TodasSaidasViewModel
            {
                Saidas = saidas,
                MesSelecionado = mes,
                AnoSelecionado = ano,
                PaginaAtual = pagina,
                TotalPaginas = totalPaginas
            };
        }

        public Task<List<DateTime>> ListarMesesDisponiveis(Usuario usuario)
        {
            var dataCadastro = usuario.DataCadastro == default
                ? DateTime.Now
                : usuario.DataCadastro;

            var primeiroMes = new DateTime(dataCadastro.Year, dataCadastro.Month, 1);
            var mesAtual = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var meses = new List<DateTime>();

            for (var mes = primeiroMes; mes <= mesAtual; mes = mes.AddMonths(1))
            {
                meses.Add(mes);
            }

            return Task.FromResult(meses);
        }

        /// <summary>
        /// Preenche nome da conta/categoria e o "Saldo restante": quanto sobrou
        /// na conta logo depois de cada saída.
        /// </summary>
        private async Task PreencherDadosDeExibicao(Usuario usuario, List<Saida> saidas)
        {
            if (saidas.Count == 0)
            {
                return;
            }

            var contas = await _context.Contas
                .Where(x => x.UsuarioId == usuario.Id)
                .ToListAsync();

            var categorias = await _context.Categorias
                .Where(x => x.UsuarioId == usuario.Id)
                .ToListAsync();

            foreach (var saida in saidas)
            {
                var conta = contas.FirstOrDefault(c => c.Id == saida.ContaId);

                saida.NomeConta = conta?.NomeConta ?? "Conta removida";
                saida.NomeCategoria = categorias
                    .FirstOrDefault(c => c.Id == saida.CategoriaId)?.NomeCategoria ?? string.Empty;

                if (conta == null)
                {
                    continue;
                }

                var entradasAte = await _context.Entradas
                    .Where(x =>
                        x.UsuarioId == usuario.Id &&
                        x.ContaId == saida.ContaId &&
                        x.DataEntrada <= saida.DataSaida)
                    .SumAsync(x => (decimal?)x.ValorEntrada) ?? 0m;

                var saidasAte = await _context.Saidas
                    .Where(x =>
                        x.UsuarioId == usuario.Id &&
                        x.ContaId == saida.ContaId &&
                        (x.DataSaida < saida.DataSaida ||
                         (x.DataSaida == saida.DataSaida && x.Id <= saida.Id)))
                    .SumAsync(x => (decimal?)x.ValorSaida) ?? 0m;

                saida.SaldoRestante = conta.SaldoInicial + entradasAte - saidasAte;
            }
        }
    }
}
