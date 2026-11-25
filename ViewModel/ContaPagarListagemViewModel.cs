namespace lsfinan.ViewModel
{
    public class ContaPagarListagemViewModel
    {
        /// <summary>"pendentes" ou "pagas".</summary>
        public string Filtro { get; set; } = "pendentes";

        public List<ContaPagarItemViewModel> Atrasadas { get; set; } = new();
        public List<ContaPagarItemViewModel> VencemHoje { get; set; } = new();
        public List<ContaPagarItemViewModel> ProximosDias { get; set; } = new();
        public List<ContaPagarItemViewModel> Futuras { get; set; } = new();
        public List<ContaPagarItemViewModel> Pagas { get; set; } = new();

        public int JanelaAlertaDias { get; set; }

        public decimal TotalAtrasadas => Atrasadas.Sum(x => x.Valor);
        public decimal TotalHoje => VencemHoje.Sum(x => x.Valor);
        public decimal TotalProximos => ProximosDias.Sum(x => x.Valor);
        public decimal TotalPendente =>
            TotalAtrasadas + TotalHoje + TotalProximos + Futuras.Sum(x => x.Valor);

        public int QuantidadePendentes =>
            Atrasadas.Count + VencemHoje.Count + ProximosDias.Count + Futuras.Count;
    }
}
