using lsfinan.Interfaces;
using lsfinan.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace lsfinan.Controllers
{

    public class DashboardController : Controller
    {

        private readonly IDashboardService _dashboardService;
        private readonly UserManager<Usuario> _userManager;
         private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, UserManager<Usuario> userManager, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
            _logger = logger;
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

            var dashboardViewModel =
                await _dashboardService.ObterDashboardAsync(usuario.Id);

            return View(dashboardViewModel);
        }
    }
}