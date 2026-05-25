
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStore.Services.Interfaces;
using TechStore.Utilities;

namespace TechStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // FIX 10: Controller is now async — awaits the single batched + cached DB call
        public async Task<IActionResult> Index()
        {
            var stats = await _dashboardService.GetStatsAsync();

            ViewBag.Orders = stats.TotalOrders;
            ViewBag.ApprovedOrders = stats.ApprovedOrders;
            ViewBag.Users = stats.TotalUsers;
            ViewBag.Products = stats.TotalProducts;

            return View();
        }
    }
}
