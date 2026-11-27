using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace lsfinan.Controllers
{
    [Route("[controller]")]
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ICadastroPendenteService _cadastroPendenteService;
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;

        public UsuarioController(
            IUsuarioService usuarioService,
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            ICadastroPendenteService cadastroPendenteService)
        {
            _usuarioService = usuarioService;
            _userManager = userManager;
            _signInManager = signInManager;
            _cadastroPendenteService = cadastroPendenteService;
        }

        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet("Cadastro")]
        public IActionResult Cadastro()
        {
            return View();
        }

        [HttpGet("VerificarEmail")]
        public IActionResult VerificarEmail()
        {
            var email = TempData.Peek("EmailConfirmacao")?.ToString();

            if (string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(Cadastro));

            return View(new VerificarEmailViewModel
            {
                Email = email
            });
        }

        [HttpPost("Cadastro")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastro(UsuarioCadastroViewModel usuario)
        {
            if (!ModelState.IsValid)
                return View(usuario);

            var resultado = await _cadastroPendenteService
                .IniciarCadastroAsync(usuario);

            if (resultado.Succeeded)
            {
                TempData["EmailConfirmacao"] = usuario.Email;
                return RedirectToAction(nameof(VerificarEmail));
            }

            foreach (var erro in resultado.Errors)
                ModelState.AddModelError(string.Empty, erro.Description);

            return View(usuario);
        }

        [HttpPost("VerificarEmail")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerificarEmail(
    VerificarEmailViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var resultado = await _cadastroPendenteService
                .ConfirmarCadastroAsync(model.Email, model.Codigo);

            if (!resultado.Succeeded)
            {
                foreach (var erro in resultado.Errors)
                    ModelState.AddModelError(string.Empty, erro.Description);

                return View(model);
            }

            TempData["Sucesso"] = "E-mail confirmado! Agora você já pode entrar.";

            return RedirectToAction(nameof(Login));
        }

        [HttpPost("ReenviarCodigo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReenviarCodigo(string email)
        {
            try
            {
                await _cadastroPendenteService.ReenviarCodigoAsync(email);
                TempData["Sucesso"] = "Um novo código foi enviado.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            TempData["EmailConfirmacao"] = email;

            return RedirectToAction(nameof(VerificarEmail));
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromForm] UsuarioLoginViewModel usuario)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _usuarioService.LoginUsuario(usuario);

                if (resultado.Succeeded)
                {
                    TempData["MostrarBoasVindas"] = true;
                    // mostra o aviso de contas atrasadas / a vencer assim que entra
                    TempData["MostrarAlertaContas"] = true;

                    return RedirectToAction("Index", "Dashboard");
                }

                ModelState.AddModelError(
                    string.Empty,
                    "Email ou senha inválidos"
                );
            }

            return View(usuario);
        }

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario != null)
            {
                TempData["NomeUsuarioLogout"] = usuario.Nome;
            }

            await _usuarioService.LogoutUsuario();

            TempData["MostrarDespedida"] = true;

            return RedirectToAction("Login", "Usuario");
        }

        // ------------------------------------------------------------------
        // PERFIL (o link "Minha conta" do menu apontava para uma página que não existia)
        // ------------------------------------------------------------------

        [HttpGet("Perfil")]
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            return View(MontarPerfil(usuario));
        }

        [HttpPost("Perfil")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(PerfilViewModel perfil)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            // só o nome é editado aqui; a troca de senha tem o próprio formulário
            ModelState.Remove(nameof(PerfilViewModel.SenhaAtual));
            ModelState.Remove(nameof(PerfilViewModel.NovaSenha));
            ModelState.Remove(nameof(PerfilViewModel.ConfirmarNovaSenha));

            if (!ModelState.IsValid)
            {
                perfil.Email = usuario.Email ?? string.Empty;
                perfil.DataCadastro = usuario.DataCadastro;
                return View(perfil);
            }

            usuario.Nome = perfil.Nome.Trim();
            await _userManager.UpdateAsync(usuario);

            TempData["Sucesso"] = "Seus dados foram atualizados.";
            return RedirectToAction(nameof(Perfil));
        }

        [HttpPost("AlterarSenha")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarSenha(PerfilViewModel perfil)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var resposta = MontarPerfil(usuario);
            ModelState.Clear();

            if (string.IsNullOrEmpty(perfil.SenhaAtual) ||
                string.IsNullOrEmpty(perfil.NovaSenha) ||
                string.IsNullOrEmpty(perfil.ConfirmarNovaSenha))
            {
                ModelState.AddModelError("Senha", "Preencha a senha atual, a nova senha e a confirmação.");
                return View("Perfil", resposta);
            }

            if (perfil.NovaSenha != perfil.ConfirmarNovaSenha)
            {
                ModelState.AddModelError("Senha", "A confirmação não confere com a nova senha.");
                return View("Perfil", resposta);
            }

            var resultado = await _userManager.ChangePasswordAsync(
                usuario, perfil.SenhaAtual, perfil.NovaSenha);

            if (!resultado.Succeeded)
            {
                foreach (var erro in resultado.Errors)
                {
                    var mensagem = erro.Code switch
                    {
                        "PasswordMismatch" => "A senha atual está incorreta.",
                        "PasswordTooShort" => "A nova senha precisa ter pelo menos 6 caracteres.",
                        _ => erro.Description
                    };

                    ModelState.AddModelError("Senha", mensagem);
                }

                return View("Perfil", resposta);
            }

            await _signInManager.RefreshSignInAsync(usuario);

            TempData["Sucesso"] = "Senha alterada com sucesso.";
            return RedirectToAction(nameof(Perfil));
        }

        private static PerfilViewModel MontarPerfil(Usuario usuario) => new()
        {
            Nome = usuario.Nome,
            Email = usuario.Email ?? string.Empty,
            DataCadastro = usuario.DataCadastro
        };
    }
}