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
    public class CategoriaController : Controller
    {
        private readonly ICategoriaService _categoriaService;
        private readonly UserManager<Usuario> _userManager;

        public CategoriaController(ICategoriaService categoria, UserManager<Usuario> userManager)
        {
            _categoriaService = categoria;
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

            var categorias = await _categoriaService.ListarCategorias(usuario);
            return View(categorias);
        }

        [HttpGet]
        public IActionResult CadastrarCategoria()
        {
            return View(new CategoriaViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarCategoria(CategoriaViewModel categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            try
            {
                await _categoriaService.CadastrarCategoria(categoria, usuario);
            }
            catch (RegraDeNegocioException ex)
            {
                // ex.: "A categoria X já existe." aparece no próprio formulário
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(categoria);
            }

            TempData["Sucesso"] = "Categoria cadastrada com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            try
            {
                var categoria = await _categoriaService.ObterParaEdicao(id, usuario);
                return View("CadastrarCategoria", categoria);
            }
            catch (RegraDeNegocioException ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Editar(int id, CategoriaViewModel categoria)
        {
            categoria.Id = id;

            if (!ModelState.IsValid)
            {
                return View("CadastrarCategoria", categoria);
            }

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            try
            {
                await _categoriaService.AtualizarCategoria(id, categoria, usuario);
            }
            catch (RegraDeNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("CadastrarCategoria", categoria);
            }

            TempData["Sucesso"] = "Categoria atualizada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            try
            {
                await _categoriaService.Excluir(id, usuario);
                TempData["Sucesso"] = "Categoria excluída.";
            }
            catch (RegraDeNegocioException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
