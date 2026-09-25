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
        private readonly UserManager<Usuario> _userManager;

        public UsuarioController(IUsuarioService usuarioService, UserManager<Usuario> userManager)
        {
            _usuarioService = usuarioService;
            _userManager = userManager;
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

        [HttpPost("Cadastro")]
        public async Task<IActionResult> Cadastro(
         UsuarioCadastroViewModel usuario)
        {
            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            var resultado = await _usuarioService.CadastrarUsuario(usuario);

            if (resultado.Succeeded)
            {
                TempData["MostrarBoasVindas"] = true;
                var usuarioCadastrado = await _usuarioService.LoginUsuario(new UsuarioLoginViewModel
                {
                    Email = usuario.Email,
                    Senha = usuario.Senha
                });
                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var erro in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, erro.Description);
            }

            return View(usuario);
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
    }
}