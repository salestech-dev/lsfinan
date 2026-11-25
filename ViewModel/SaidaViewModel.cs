using System.ComponentModel.DataAnnotations;
using lsfinan.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace lsfinan.ViewModel
{
    public class SaidaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome da saída.")]
        [StringLength(120, ErrorMessage = "O nome pode ter no máximo 120 caracteres.")]
        public string NomeSaida { get; set; } = string.Empty;

        [Range(0.01, 999999999, ErrorMessage = "Informe um valor maior que zero.")]
        public decimal ValorSaida { get; set; }

        public string? DescricaoSaida { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecione uma categoria.")]
        public int CategoriaId { get; set; }

        public int ContaId { get; set; }

        /// <summary>
        /// Preenchida quando o saldo de uma conta não basta e o usuário
        /// escolhe outra conta ou divide o valor entre várias.
        /// </summary>
        public List<AlocacaoPagamentoViewModel> Alocacoes { get; set; } = new();

        [ValidateNever]
        public List<Categoria> Categorias { get; set; } = new();

        [ValidateNever]
        public List<Conta> Contas { get; set; } = new();
    }
}
