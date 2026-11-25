using System.ComponentModel.DataAnnotations;

namespace lsfinan.ViewModel
{
    public class PerfilViewModel
    {
        [Required(ErrorMessage = "Informe o seu nome.")]
        [StringLength(80, ErrorMessage = "O nome pode ter no máximo 80 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime DataCadastro { get; set; }

        // ---- Troca de senha (validada na própria ação) ----
        [DataType(DataType.Password)]
        public string? SenhaAtual { get; set; }

        [DataType(DataType.Password)]
        public string? NovaSenha { get; set; }

        [DataType(DataType.Password)]
        public string? ConfirmarNovaSenha { get; set; }
    }
}
