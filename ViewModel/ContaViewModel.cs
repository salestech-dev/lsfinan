using System.ComponentModel.DataAnnotations;
using lsfinan.Enum;

namespace lsfinan.ViewModel
{
    public class ContaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome da conta.")]
        [StringLength(60, ErrorMessage = "O nome pode ter no máximo 60 caracteres.")]
        public string NomeConta { get; set; } = string.Empty;

        public decimal SaldoInicial { get; set; }

        [Required(ErrorMessage = "Selecione o tipo da conta.")]
        public EnumTipoConta TipoConta { get; set; }

        public bool Ativa { get; set; } = true;
    }
}
