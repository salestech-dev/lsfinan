
using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace lsfinan.Controllers
{
    public class ContaController : Controller
    {
        private readonly IContaService _contaService;
        private readonly UserManager<Usuario> _userManager;

        public ContaController(IContaService contaService, UserManager<Usuario> userManager)
        {
            _userManager = userManager;
            _contaService = contaService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            var contas = await _contaService.ListarContas(usuario);
            return View(contas);
        }

        [HttpGet]
        [Authorize]
        public IActionResult CadastrarConta()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CadastrarConta(ContaViewModel contaViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(contaViewModel);
            }

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            await _contaService.CadastrarConta(contaViewModel, usuario);
            TempData["ContaCriada"] = "Conta criada com sucesso!";
            return RedirectToAction(nameof(Index));

        }

    }
}