    using System.Threading.Tasks;
    using lsfinan.Interfaces;
    using lsfinan.Models;
    using lsfinan.ViewModel;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;


    namespace lsfinan.Controllers
    {

        public class CategoriaController : Controller
        {
            private readonly ICategoriaService categoriaService;
            private readonly UserManager<Usuario> _userManager;
            public CategoriaController(ICategoriaService categoria, UserManager<Usuario> userManager)
            {
                categoriaService = categoria;
                _userManager = userManager;
            }

            [Authorize]
            [HttpGet]
            public async Task<IActionResult> Index()
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null)
                {
                    return Challenge();
                }

                var categoriaViewModel = await categoriaService.ListarCategorias(usuario);
                return View(categoriaViewModel);

            }

            [HttpGet]
            [Authorize]
            public IActionResult CadastrarCategoria()
            {
                return View();
            }

            [HttpPost]
            [Authorize]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> CadastrarCategoria(
        CategoriaViewModel categoria)
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

                await categoriaService.CadastrarCategoria(categoria, usuario);

                return RedirectToAction(nameof(Index));
            }
        }
    }