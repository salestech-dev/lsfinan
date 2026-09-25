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

        public DashboardController(IDashboardService dashboardService, UserManager<Usuario> userManager)
        {
            _dashboardService = dashboardService;
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

            var dashboardViewModel =
                await _dashboardService.ObterDashboardAsync(usuario.Id);

            return View(dashboardViewModel);
        }
    }
}