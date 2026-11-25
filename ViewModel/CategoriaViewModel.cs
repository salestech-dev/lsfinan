using System.ComponentModel.DataAnnotations;

namespace lsfinan.ViewModel
{
    public class CategoriaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome da categoria.")]
        [StringLength(60, ErrorMessage = "O nome pode ter no máximo 60 caracteres.")]
        public string NomeDaCategoria { get; set; } = string.Empty;

        public bool Ativa { get; set; } = true;
    }
}
