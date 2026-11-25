public class MovimentacaoViewModel
{
    public string Descricao { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public DateTime Data { get; set; }

    /// <summary>"Entrada" ou "Saída".</summary>
    public string Tipo { get; set; } = string.Empty;

    public string Conta { get; set; } = string.Empty;
}
