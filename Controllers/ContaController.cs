using lsfinan.Exceptions;
using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace lsfinan.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
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
        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            var contas = await _contaService.ListarContasComSaldo(usuario);
            return View(contas);
        }

        [HttpGet]
        public IActionResult CadastrarConta()
        {
            return View(new ContaViewModel());
        }

        [HttpPost]
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

            try
            {
                await _contaService.CadastrarConta(contaViewModel, usuario);
            }
            catch (RegraDeNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(contaViewModel);
            }

            TempData["ContaCriada"] = "Conta criada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditarConta(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            try
            {
                var conta = await _contaService.ObterParaEdicao(id, usuario);
                return View("CadastrarConta", conta);
            }
            catch (RegraDeNegocioException ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarConta(int id, ContaViewModel contaViewModel)
        {
            contaViewModel.Id = id;

            if (!ModelState.IsValid)
            {
                return View("CadastrarConta", contaViewModel);
            }

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            try
            {
                await _contaService.EditarConta(id, contaViewModel, usuario);
            }
            catch (RegraDeNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("CadastrarConta", contaViewModel);
            }

            TempData["Sucesso"] = "Conta atualizada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
