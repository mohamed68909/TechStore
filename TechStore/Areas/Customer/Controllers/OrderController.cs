using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;
using TechStore.Services.Interfaces;

namespace TechStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult MyOrders()
        {
            var claimsIdentity = (ClaimsIdentity?)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var orders = _orderService.GetUserOrders(userId);
            return View(orders);
        }
        [HttpGet]
        public IActionResult Details(int orderid)
        {
            // ??? ?????? ????? ??? ??????
            var orderVM = _orderService.GetOrderDetails(orderid);
            if (orderVM == null || orderVM.OrderHeader == null)
            {
                return NotFound();
            }

            // Verify Ownership
            var claimsIdentity = (ClaimsIdentity?)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            if (orderVM.OrderHeader.ApplicationUserId != userId)
            {
                return Forbid();
            }

            return View(orderVM);
        }
    }
}
