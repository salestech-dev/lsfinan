namespace lsfinan.ViewModel
{
    /// <summary>Resumo usado pelo aviso de login, pelo menu e pelo dashboard.</summary>
    public class AlertasContasPagarViewModel
    {
        public int JanelaDias { get; set; }

        public int QtdAtrasadas { get; set; }
        public decimal TotalAtrasadas { get; set; }

        public int QtdHoje { get; set; }
        public decimal TotalHoje { get; set; }

        /// <summary>Vencem de amanhã até o fim da janela de alerta.</summary>
        public int QtdProximos { get; set; }
        public decimal TotalProximos { get; set; }

        /// <summary>As mais urgentes primeiro: atrasadas, hoje e depois as próximas.</summary>
        public List<ContaPagarItemViewModel> Itens { get; set; } = new();

        public int Badge => QtdAtrasadas + QtdHoje + QtdProximos;

        public bool TemAlerta => Badge > 0;

        public bool TemAtraso => QtdAtrasadas > 0;
    }
}
