
using System.ComponentModel.DataAnnotations;

namespace lsfinan.ViewModel
{
    public class VerificarEmailViewModel
    {
        [Required(ErrorMessage = "Informe o código enviado ao seu e-mail.")]
        [StringLength(6, MinimumLength = 6,
            ErrorMessage = "O código deve conter 6 números.")]
        [RegularExpression(@"^\d{6}$",
            ErrorMessage = "O código deve conter apenas números.")]
        public string Codigo { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}
