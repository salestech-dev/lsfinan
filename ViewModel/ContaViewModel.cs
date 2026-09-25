using lsfinan.Enum;

namespace lsfinan.ViewModel
{
    public class ContaViewModel
    {
        public string NomeConta { get; set; } = string.Empty;
        public decimal SaldoInicial { get; set; }
        public EnumTipoConta TipoConta { get; set; }
    }
}