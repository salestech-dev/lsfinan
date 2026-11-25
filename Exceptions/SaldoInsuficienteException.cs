using lsfinan.ViewModel;

namespace lsfinan.Exceptions
{
    /// <summary>
    /// A conta escolhida não tem saldo para cobrir o valor.
    /// Carrega tudo que a tela precisa para oferecer outra conta
    /// ou a divisão do valor entre duas ou mais contas.
    /// </summary>
    public class SaldoInsuficienteException : RegraDeNegocioException
    {
        public int ContaId { get; }
        public string NomeConta { get; }
        public decimal SaldoConta { get; }
        public decimal ValorSolicitado { get; }
        public List<ContaListagemViewModel> ContasAtivas { get; }
        public List<AlocacaoPagamentoViewModel> Sugestao { get; }

        public decimal Falta => Math.Max(0m, ValorSolicitado - SaldoConta);

        public decimal SaldoTotalDisponivel =>
            ContasAtivas.Sum(c => Math.Max(0m, c.SaldoAtual));

        public bool OutrasContasCobrem => SaldoTotalDisponivel >= ValorSolicitado;

        public SaldoInsuficienteException(
            int contaId,
            string nomeConta,
            decimal saldoConta,
            decimal valorSolicitado,
            List<ContaListagemViewModel> contasAtivas,
            List<AlocacaoPagamentoViewModel> sugestao)
            : base($"A conta \"{nomeConta}\" não tem saldo suficiente para este valor.")
        {
            ContaId = contaId;
            NomeConta = nomeConta;
            SaldoConta = saldoConta;
            ValorSolicitado = valorSolicitado;
            ContasAtivas = contasAtivas;
            Sugestao = sugestao;
        }
    }
}
