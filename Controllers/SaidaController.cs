using lsfinan.Exceptions;
using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.Services;
using lsfinan.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace lsfinan.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class SaidaController : Controller
    {
        private readonly ISaidaService _saidaService;
        private readonly ICategoriaService _categoriaService;
        private readonly IContaService _contaService;
        private readonly UserManager<Usuario> _userManager;

        public SaidaController(
            ISaidaService saidaService,
            ICategoriaService categoriaService,
            IContaService contaService,
            UserManager<Usuario> userManager)
        {
            _saidaService = saidaService;
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

            var saidas = await _saidaService.ListarSaidasDeHoje(usuario);

            return View(saidas);
        }

        // ------------------------------------------------------------------
        // CADASTRAR
        // ------------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> CadastrarSaida()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var viewModel = new SaidaViewModel();
            await PreencherListas(viewModel, usuario);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarSaida(SaidaViewModel saidaViewModel)
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
                    var saidas = await _saidaService.CadastrarSaida(usuario, saidaViewModel);

                    TempData["Sucesso"] = saidas.Count > 1
                        ? $"Saída registrada, dividida entre {saidas.Count} contas."
                        : "Saída registrada com sucesso.";

                    return RedirectToAction(nameof(Index));
                }
                catch (SaldoInsuficienteException ex)
                {
                    // Regra: a conta não fica negativa. Abre o painel para o
                    // usuário escolher outra conta ou dividir entre duas ou mais.
                    ViewBag.Divisao = new DivisaoPagamentoViewModel
                    {
                        Valor = ex.ValorSolicitado,
                        Contas = ex.ContasAtivas,
                        Alocacoes = ex.Sugestao,
                        NomeContaEscolhida = ex.NomeConta,
                        SaldoContaEscolhida = ex.SaldoConta
                    };
                }
                catch (RegraDeNegocioException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);

                    // erro vindo da divisão escolhida: reabre o painel com o que ele digitou
                    if (saidaViewModel.Alocacoes.Any(a => a.Valor > 0m))
                    {
                        ViewBag.Divisao = new DivisaoPagamentoViewModel
                        {
                            Valor = saidaViewModel.ValorSaida,
                            Contas = await _contaService.ListarContasAtivasComSaldo(usuario),
                            Alocacoes = saidaViewModel.Alocacoes,
                            Mensagem = ex.Message
                        };
                    }
                }
            }

            await PreencherListas(saidaViewModel, usuario);
            return View(saidaViewModel);
        }

        // ------------------------------------------------------------------
        // EDITAR / EXCLUIR
        // ------------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> EditarSaida(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            try
            {
                var saida = await _saidaService.ObterParaEdicao(id, usuario);
                return View("CadastrarSaida", saida);
            }
            catch (RegraDeNegocioException ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarSaida(int id, SaidaViewModel saidaViewModel)
        {
            saidaViewModel.Id = id;

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _saidaService.EditarSaida(id, saidaViewModel, usuario);

                    TempData["Sucesso"] = "Saída atualizada.";
                    return RedirectToAction(nameof(Index));
                }
                catch (RegraDeNegocioException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            await PreencherListas(saidaViewModel, usuario);
            return View("CadastrarSaida", saidaViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ExcluirSaida(int id, string? retorno)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            try
            {
                await _saidaService.ExcluirSaida(id, usuario);
                TempData["Sucesso"] = "Saída excluída. O valor voltou para a conta.";
            }
            catch (RegraDeNegocioException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            // volta para a lista de onde o usuário veio (só endereços do próprio site)
            if (!string.IsNullOrEmpty(retorno) && Url.IsLocalUrl(retorno))
            {
                return LocalRedirect(retorno);
            }

            return RedirectToAction(nameof(Index));
        }

        // ------------------------------------------------------------------
        // HISTÓRICO
        // ------------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> TodasSaidas(
            int? mes,
            int? ano,
            string? mesAno,
            int pagina = 1)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            // O seletor de mês e a paginação mandam mes/ano; "mesAno" (ex.: 3-2026)
            // continua aceito para links antigos. Qualquer valor inválido cai no mês atual.
            if ((mes == null || ano == null) && !string.IsNullOrEmpty(mesAno))
            {
                var partes = mesAno.Split('-');

                if (partes.Length == 2 &&
                    int.TryParse(partes[0], out var mesLido) &&
                    int.TryParse(partes[1], out var anoLido))
                {
                    mes = mesLido;
                    ano = anoLido;
                }
            }

            var mesSelecionado = mes ?? DateTime.Now.Month;
            var anoSelecionado = ano ?? DateTime.Now.Year;

            if (mesSelecionado < 1 || mesSelecionado > 12 ||
                anoSelecionado < 2000 || anoSelecionado > 2100)
            {
                mesSelecionado = DateTime.Now.Month;
                anoSelecionado = DateTime.Now.Year;
            }

            var mesesDisponiveis = await _saidaService.ListarMesesDisponiveis(usuario);

            var resultado = await _saidaService.ListarTodasSaidas(
                usuario,
                mesSelecionado,
                anoSelecionado,
                pagina);

            resultado.MesesDisponiveis = mesesDisponiveis;

            return View(resultado);
        }

        // ------------------------------------------------------------------

        /// <summary>
        /// Só categorias e contas ativas aparecem. Na edição, a que já estava
        /// escolhida continua na lista mesmo se tiver sido inativada depois.
        /// </summary>
        private async Task PreencherListas(SaidaViewModel viewModel, Usuario usuario)
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
