using lsfinan.Enum;

namespace lsfinan.ViewModel
{
    public class ContaPagarItemViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public string NomeCategoria { get; set; } = string.Empty;
        public bool Pago { get; set; }
        public DateTime? DataPagamento { get; set; }

        /// <summary>Negativo = quantos dias de atraso.</summary>
        public int DiasParaVencer { get; set; }
        public EnumStatusContaPagar Status { get; set; }
        public string TextoVencimento { get; set; } = string.Empty;

        /// <summary>Contas usadas no pagamento (só quando já está paga).</summary>
        public string ContasPagamento { get; set; } = string.Empty;
    }
}
