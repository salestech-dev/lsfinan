namespace lsfinan.ViewModel
{
    /// <summary>
    /// Quanto de um pagamento sai de cada conta. Permite pagar uma despesa
    /// com uma conta só ou dividir o valor entre duas ou mais contas.
    /// </summary>
    public class AlocacaoPagamentoViewModel
    {
        public int ContaId { get; set; }

        public decimal Valor { get; set; }
    }
}
