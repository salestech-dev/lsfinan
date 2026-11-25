using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace lsfinan.ViewModel
{
    /// <summary>Formulário de cadastro e edição de conta a pagar.</summary>
    public class ContaPagarViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome da conta.")]
        [StringLength(120, ErrorMessage = "O nome pode ter no máximo 120 caracteres.")]
        public string NomeContaPagar { get; set; } = string.Empty;

        public string? DescricaoContaPagar { get; set; }

        [Range(0.01, 999999999, ErrorMessage = "Informe um valor maior que zero.")]
        public decimal ValorContaPagar { get; set; }

        [Required(ErrorMessage = "Informe a data de vencimento.")]
        public DateTime? DataVencimento { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecione uma categoria.")]
        public int CategoriaId { get; set; }

        [ValidateNever]
        public List<Categoria> Categorias { get; set; } = new();
    }
}
