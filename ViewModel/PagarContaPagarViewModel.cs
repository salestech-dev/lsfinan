using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace lsfinan.ViewModel
{
    public class PagarContaPagarViewModel
    {
        public int Id { get; set; }

        [ValidateNever]
        public ContaPagarItemViewModel Conta { get; set; } = new();

        [ValidateNever]
        public List<ContaListagemViewModel> ContasAtivas { get; set; } = new();

        /// <summary>Quanto sai de cada conta (uma só ou várias).</summary>
        public List<AlocacaoPagamentoViewModel> Alocacoes { get; set; } = new();

        public decimal SaldoTotalDisponivel =>
            ContasAtivas.Sum(c => Math.Max(0m, c.SaldoAtual));
    }
}
