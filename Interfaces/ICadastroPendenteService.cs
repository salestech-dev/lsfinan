
using lsfinan.Models;
using lsfinan.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace lsfinan.Interfaces
{
    public interface ICadastroPendenteService
    {
        Task<IdentityResult> IniciarCadastroAsync(
            UsuarioCadastroViewModel cadastro);

        Task<IdentityResult> ConfirmarCadastroAsync(
            string email,
            string codigo);

        Task ReenviarCodigoAsync(string email);
    }
}
