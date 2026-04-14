

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

        public IActionResult Index()
        {

            ViewBag.Orders = _dashboardService.GetTotalOrdersCount();
            ViewBag.ApprovedOrders = _dashboardService.GetApprovedOrdersCount();
            ViewBag.Users = _dashboardService.GetTotalUsersCount();
            ViewBag.Products = _dashboardService.GetTotalProductsCount();

            return View();
        }
    }
}
