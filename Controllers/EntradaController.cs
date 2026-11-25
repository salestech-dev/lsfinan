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
        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var entradas = await _entradaService.ListarTodasEntradas(usuario);

            return View(entradas);
        }

        [HttpGet]
        public async Task<IActionResult> CadastrarEntrada()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var viewModel = new EntradaViewModel();
            await PreencherListas(viewModel, usuario);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarEntrada(EntradaViewModel entrada)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _entradaService.CadastrarEntrada(entrada, usuario);

                    TempData["Sucesso"] = "Entrada cadastrada com sucesso.";
                    return RedirectToAction(nameof(Index));
                }
                catch (RegraDeNegocioException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            await PreencherListas(entrada, usuario);
            return View(entrada);
        }

        [HttpGet]
        public async Task<IActionResult> EditarEntrada(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            try
            {
                var entrada = await _entradaService.ObterParaEdicao(id, usuario);
                return View("CadastrarEntrada", entrada);
            }
            catch (RegraDeNegocioException ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarEntrada(int id, EntradaViewModel entrada)
        {
            entrada.Id = id;

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _entradaService.EditarEntrada(id, entrada, usuario);

                    TempData["Sucesso"] = "Entrada atualizada.";
                    return RedirectToAction(nameof(Index));
                }
                catch (RegraDeNegocioException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            await PreencherListas(entrada, usuario);
            return View("CadastrarEntrada", entrada);
        }

        [HttpPost]
        public async Task<IActionResult> ExcluirEntrada(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            try
            {
                await _entradaService.ExcluirEntrada(id, usuario);
                TempData["Sucesso"] = "Entrada excluída.";
            }
            catch (RegraDeNegocioException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Só categorias e contas ativas aparecem. Na edição, a que já estava
        /// escolhida continua na lista mesmo se tiver sido inativada depois.
        /// </summary>
        private async Task PreencherListas(EntradaViewModel viewModel, Usuario usuario)
        {
            var categorias = await _categoriaService.ListarCategorias(usuario);
            var contas = await _contaService.ListarContas(usuario);

            viewModel.Categorias = categorias
                .Where(c => c.Ativa || c.Id == viewModel.CategoriaId)
                .ToList();

            viewModel.Contas = contas
                .Where(c => c.Ativa || c.Id == viewModel.ContaId)
                .ToList();
        }
    }
}
