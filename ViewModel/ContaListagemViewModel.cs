using lsfinan.Enum;

namespace lsfinan.ViewModel
{
    public class ContaListagemViewModel
    {
        public int Id { get; set; }
        public string NomeConta { get; set; } = string.Empty;
        public EnumTipoConta TipoConta { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal SaldoAtual { get; set; }
        public bool Ativa { get; set; }
    }
}