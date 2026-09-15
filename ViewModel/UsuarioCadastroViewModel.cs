using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace lsfinan.ViewModel
{
    public class UsuarioCadastroViewModel
    {
        [Required(ErrorMessage = "Digite pelo menos o seu primeiro nome.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Digite um e-mail válido.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Digite uma senha.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a senha.")]
        [DataType(DataType.Password)]
        [Compare("Senha", ErrorMessage = "As senhas não conferem.")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }
}