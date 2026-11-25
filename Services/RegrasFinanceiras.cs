using System.Globalization;
using lsfinan.Enum;
using lsfinan.Exceptions;
using lsfinan.ViewModel;

namespace lsfinan.Services
{
    /// <summary>
    /// Regras de negócio puras (sem banco de dados): saldo disponível,
    /// divisão de um pagamento entre contas e situação de vencimento.
    /// Ficam separadas para serem fáceis de testar e de reaproveitar.
    /// </summary>
    public static class RegrasFinanceiras
    {
        /// <summary>Quantos dias à frente uma conta a pagar já gera aviso.</summary>
        public const int JanelaAlertaDias = 7;

        private static readonly CultureInfo PtBr = new("pt-BR");

        public static decimal Arredondar(decimal valor) =>
            Math.Round(valor, 2, MidpointRounding.AwayFromZero);

        /// <summary>Saldo negativo não conta como dinheiro disponível.</summary>
        public static decimal Disponivel(decimal saldo) =>
            Arredondar(Math.Max(0m, saldo));

        public static string Moeda(decimal valor) => valor.ToString("C2", PtBr);

        // ------------------------------------------------------------------
        // DIVISÃO DE PAGAMENTO ENTRE CONTAS
        // ------------------------------------------------------------------

        /// <summary>
        /// Junta contas repetidas, arredonda para centavos e descarta
        /// linhas sem valor (a tela manda uma linha por conta).
        /// </summary>
        public static List<AlocacaoPagamentoViewModel> NormalizarAlocacoes(
            IEnumerable<AlocacaoPagamentoViewModel>? alocacoes)
        {
            if (alocacoes == null)
            {
                return new List<AlocacaoPagamentoViewModel>();
            }

            return alocacoes
                .Where(a => a != null && a.ContaId > 0 && Arredondar(a.Valor) > 0m)
                .GroupBy(a => a.ContaId)
                .Select(g => new AlocacaoPagamentoViewModel
                {
                    ContaId = g.Key,
                    Valor = Arredondar(g.Sum(x => x.Valor))
                })
                .ToList();
        }

        /// <summary>
        /// Sugere de onde tirar o dinheiro. Se uma conta sozinha cobre tudo,
        /// sugere só ela; senão junta o menor número de contas possível,
        /// começando pela conta que o usuário tinha escolhido.
        /// </summary>
        public static List<AlocacaoPagamentoViewModel> SugerirDivisao(
            decimal valor,
            int contaPreferidaId,
            IEnumerable<ContaListagemViewModel> contasAtivas)
        {
            var valorTotal = Arredondar(valor);
            var contas = contasAtivas.Where(c => Disponivel(c.SaldoAtual) > 0m).ToList();

            var unica = contas
                .Where(c => Disponivel(c.SaldoAtual) >= valorTotal)
                .OrderByDescending(c => c.Id == contaPreferidaId)
                .ThenByDescending(c => c.SaldoAtual)
                .FirstOrDefault();

            if (unica != null)
            {
                return new List<AlocacaoPagamentoViewModel>
                {
                    new() { ContaId = unica.Id, Valor = valorTotal }
                };
            }

            var restante = valorTotal;
            var resultado = new List<AlocacaoPagamentoViewModel>();

            foreach (var conta in contas
                .OrderByDescending(c => c.Id == contaPreferidaId)
                .ThenByDescending(c => c.SaldoAtual))
            {
                if (restante <= 0m)
                {
                    break;
                }

                var usar = Math.Min(restante, Disponivel(conta.SaldoAtual));
                resultado.Add(new AlocacaoPagamentoViewModel { ContaId = conta.Id, Valor = usar });
                restante -= usar;
            }

            return resultado;
        }

        /// <summary>
        /// Confere se a divisão escolhida é válida: só contas ativas do
        /// usuário, cada conta com saldo para o que vai ceder e soma igual
        /// ao valor total.
        /// </summary>
        public static void ValidarAlocacoes(
            decimal valorTotal,
            IReadOnlyList<AlocacaoPagamentoViewModel> alocacoes,
            IReadOnlyDictionary<int, ContaListagemViewModel> contasAtivas)
        {
            if (alocacoes.Count == 0)
            {
                throw new RegraDeNegocioException(
                    "Escolha de qual conta o valor vai sair.");
            }

            foreach (var alocacao in alocacoes)
            {
                if (!contasAtivas.TryGetValue(alocacao.ContaId, out var conta))
                {
                    throw new RegraDeNegocioException(
                        "Uma das contas escolhidas não existe ou está inativa.");
                }

                var disponivel = Disponivel(conta.SaldoAtual);

                if (alocacao.Valor > disponivel)
                {
                    throw new RegraDeNegocioException(
                        $"A conta \"{conta.NomeConta}\" tem apenas {Moeda(disponivel)} " +
                        $"disponível, mas foram pedidos {Moeda(alocacao.Valor)}.");
                }
            }

            var soma = Arredondar(alocacoes.Sum(a => a.Valor));
            var total = Arredondar(valorTotal);

            if (soma != total)
            {
                throw new RegraDeNegocioException(
                    $"A soma das contas ({Moeda(soma)}) precisa ser igual ao valor " +
                    $"({Moeda(total)}).");
            }
        }

        // ------------------------------------------------------------------
        // VENCIMENTOS
        // ------------------------------------------------------------------

        public static int DiasParaVencer(DateTime vencimento, DateTime hoje) =>
            (int)(vencimento.Date - hoje.Date).TotalDays;

        public static EnumStatusContaPagar ClassificarStatus(
            bool pago,
            DateTime vencimento,
            DateTime hoje,
            int janelaDias = JanelaAlertaDias)
        {
            if (pago)
            {
                return EnumStatusContaPagar.Paga;
            }

            var dias = DiasParaVencer(vencimento, hoje);

            if (dias < 0)
            {
                return EnumStatusContaPagar.Atrasada;
            }

            if (dias == 0)
            {
                return EnumStatusContaPagar.VenceHoje;
            }

            return dias <= janelaDias
                ? EnumStatusContaPagar.VenceEmBreve
                : EnumStatusContaPagar.Pendente;
        }

        public static string TextoVencimento(bool pago, int dias)
        {
            if (pago)
            {
                return "Paga";
            }

            if (dias < 0)
            {
                var atraso = Math.Abs(dias);
                return atraso == 1 ? "Atrasada há 1 dia" : $"Atrasada há {atraso} dias";
            }

            return dias switch
            {
                0 => "Vence hoje",
                1 => "Vence amanhã",
                _ => $"Vence em {dias} dias"
            };
        }
    }
}
