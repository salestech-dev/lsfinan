using lsfinan.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace lsfinan.Interfaces
{
    public interface IUsuarioService
    {
        Task<IdentityResult> CadastrarUsuario(
            UsuarioCadastroViewModel usuario);

        Task<SignInResult> LoginUsuario(
            UsuarioLoginViewModel usuario);

        Task LogoutUsuario();
    }
}