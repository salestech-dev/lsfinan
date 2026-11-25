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
    public class ContaPagarController : Controller
    {
        private readonly IContaPagarService _contaPagarService;
        private readonly UserManager<Usuario> _userManager;

        public ContaPagarController(
            IContaPagarService contaPagarService,
            UserManager<Usuario> userManager)
        {
            _contaPagarService = contaPagarService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? filtro)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var listagem = await _contaPagarService.Listar(usuario, filtro ?? "pendentes");

            return View(listagem);
        }

        // ------------------------------------------------------------------
        // CADASTRAR
        // ------------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Cadastrar()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            return View(await _contaPagarService.NovoFormulario(usuario));
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar(ContaPagarViewModel viewModel)
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
                    await _contaPagarService.Cadastrar(viewModel, usuario);

                    TempData["Sucesso"] = "Conta a pagar cadastrada. Vamos te lembrar do vencimento.";
                    return RedirectToAction(nameof(Index));
                }
                catch (RegraDeNegocioException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            await _contaPagarService.PreencherCategorias(viewModel, usuario);
            return View(viewModel);
        }

        // ------------------------------------------------------------------
        // EDITAR / EXCLUIR
        // ------------------------------------------------------------------

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
                var conta = await _contaPagarService.ObterParaEdicao(id, usuario);
                return View("Cadastrar", conta);
            }
            catch (RegraDeNegocioException ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Editar(int id, ContaPagarViewModel viewModel)
        {
            viewModel.Id = id;

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _contaPagarService.Editar(id, viewModel, usuario);

                    TempData["Sucesso"] = "Conta a pagar atualizada.";
                    return RedirectToAction(nameof(Index));
                }
                catch (RegraDeNegocioException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            await _contaPagarService.PreencherCategorias(viewModel, usuario);
            return View("Cadastrar", viewModel);
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
                await _contaPagarService.Excluir(id, usuario);
                TempData["Sucesso"] = "Conta a pagar excluída.";
            }
            catch (RegraDeNegocioException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ------------------------------------------------------------------
        // PAGAR
        // ------------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Pagar(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            try
            {
                return View(await _contaPagarService.ObterParaPagamento(id, usuario));
            }
            catch (RegraDeNegocioException ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Pagar(
            int id,
            [Bind(Prefix = "Alocacoes")] List<AlocacaoPagamentoViewModel> alocacoes)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            try
            {
                await _contaPagarService.Pagar(id, alocacoes, usuario);

                TempData["Sucesso"] = "Pagamento registrado. O valor saiu das contas escolhidas.";
                return RedirectToAction(nameof(Index));
            }
            catch (RegraDeNegocioException ex)
            {
                // Mostra o erro e devolve a tela com o que o usuário tinha escolhido.
                PagarContaPagarViewModel viewModel;

                try
                {
                    viewModel = await _contaPagarService.ObterParaPagamento(id, usuario);
                }
                catch (RegraDeNegocioException)
                {
                    TempData["Erro"] = ex.Message;
                    return RedirectToAction(nameof(Index));
                }

                viewModel.Alocacoes = alocacoes ?? new List<AlocacaoPagamentoViewModel>();
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(viewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DesfazerPagamento(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            try
            {
                await _contaPagarService.DesfazerPagamento(id, usuario);
                TempData["Sucesso"] = "Pagamento desfeito. O valor voltou para as contas e a conta a pagar está pendente de novo.";
            }
            catch (RegraDeNegocioException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { filtro = "pagas" });
        }
    }
}
