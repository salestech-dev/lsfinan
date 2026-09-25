using lsfinan.ViewModel;

public class DashboardViewModel
{
    public string NomeUsuario { get; set; } = string.Empty;

    public decimal SaldoTotal { get; set; }

    public decimal TotalEntradas { get; set; }

    public decimal TotalSaidas { get; set; }

    public decimal Resultado { get; set; }

    public List<MovimentacaoViewModel> UltimasMovimentacoes { get; set; } = new();
}

