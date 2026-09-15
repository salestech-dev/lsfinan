using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.ViewModel;
using Microsoft.AspNetCore.Identity;

public class UsuarioService : IUsuarioService
{
    private readonly UserManager<Usuario> _userManager;

    public UsuarioService(UserManager<Usuario> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IdentityResult> CadastrarUsuario(
        UsuarioCadastroViewModel usuario)
    {
        var usuarioExistente =
            await _userManager.FindByEmailAsync(usuario.Email);

        if (usuarioExistente != null)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description = "Email já cadastrado"
                });
        }

        return await _userManager.CreateAsync(
            new Usuario
            {
                UserName = usuario.Email,
                Email = usuario.Email,
                Nome = usuario.Nome
            },
            usuario.Senha);
    }
}