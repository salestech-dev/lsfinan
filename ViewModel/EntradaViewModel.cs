using System.ComponentModel.DataAnnotations;
using lsfinan.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class EntradaViewModel
{
    public int Id { get; set; }

    public string NomeEntrada { get; set; } = string.Empty;

    [Range(0.01, 999999999, ErrorMessage = "Informe um valor maior que zero.")]
    public decimal ValorEntrada { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma categoria.")]
    public int CategoriaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma conta.")]
    public int ContaId { get; set; }

    [Required(ErrorMessage = "Informe uma descrição.")]
    [StringLength(120, ErrorMessage = "A descrição pode ter no máximo 120 caracteres.")]
    public string DescricaoEntrada { get; set; } = string.Empty;


    // Listas para os selects da tela
    [ValidateNever]
    public List<Categoria> Categorias { get; set; } = new();

    [ValidateNever]
    public List<Conta> Contas { get; set; } = new();

}
