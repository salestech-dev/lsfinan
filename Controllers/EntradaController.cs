using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace lsfinan.Controllers
{
    public class EntradaController : Controller
    {
        private readonly IEntradaService _entradaService;
        private readonly ICategoriaService _categoriaService;
        private readonly IContaService _contaService;
        private readonly UserManager<Usuario> _userManager;

        public EntradaController(
            IEntradaService entradaService,
            ICategoriaService categoriaService,
            IContaService contaService,
            UserManager<Usuario> userManager)
        {
            _entradaService = entradaService;
            _categoriaService = categoriaService;
            _contaService = contaService;
            _userManager = userManager;
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

            var entradas = await _entradaService.ListarEntradas(usuario);

            return View(entradas);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CadastrarEntrada()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var categorias = await _categoriaService.ListarCategorias(usuario);

            var contas = await _contaService.ListarContas(usuario);

            var viewModel = new EntradaViewModel
            {
                Categorias = categorias,
                Contas = contas
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CadastrarEntrada(EntradaViewModel entrada)
        {
            if (!ModelState.IsValid)
            {
                return View(entrada);
            }

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            await _entradaService.CadastrarEntrada(entrada, usuario);

            return RedirectToAction("Index");
        }
    }
}