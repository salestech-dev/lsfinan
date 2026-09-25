using lsfinan.Models;

public class EntradaViewModel
{
    public string NomeEntrada { get; set; } = string.Empty;

    public decimal ValorEntrada { get; set; }

    public DateTime DataEntrada { get; set; }

    public int CategoriaId { get; set; }

    public int ContaId { get; set; }

    public string DescricaoEntrada { get; set; } = string.Empty;

    public bool ReceberDepois { get; set; }

    public DateTime? DataRecebimento { get; set; }

    // Listas para os selects da tela
    public List<Categoria> Categorias { get; set; } = new();

    public List<Conta> Contas { get; set; } = new();

}