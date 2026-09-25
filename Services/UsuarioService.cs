using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.ViewModel;
using Microsoft.AspNetCore.Identity;

public class UsuarioService : IUsuarioService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;

    public UsuarioService(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
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

    public async Task LogoutUsuario()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<SignInResult> LoginUsuario(UsuarioLoginViewModel usuario)
    {
        var usuarioExistente = await _userManager.FindByEmailAsync(usuario.Email);

        if (usuarioExistente == null)
        {
            return SignInResult.Failed;
        }

        var resultado = await _signInManager.PasswordSignInAsync(usuarioExistente, usuario.Senha, false, false);

        return resultado;
    }
}