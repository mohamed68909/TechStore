using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStore.Entities.ViewModels;
using TechStore.Services.Interfaces;
using TechStore.Utilities;


namespace TechStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        [BindProperty]
        public OrderVM OrderVM { get; set; } = default!;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult GetData()
        {
            var orders = _orderService.GetAllOrders();
            return Json(new { data = orders });
        }

        public IActionResult Details(int orderid)
        {
            var vm = _orderService.GetOrderDetails(orderid);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails()
        {
            _orderService.UpdateOrderDetails(OrderVM.OrderHeader);
            TempData["Update"] = "Order Updated Successfully";
            return RedirectToAction("Details", new { orderid = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // FIX 11: Renamed from StartProccess → StartProcess; SD.Proccessing → SD.Processing
        public IActionResult StartProcess()
        {
            _orderService.UpdateStatus(OrderVM.OrderHeader.Id, SD.Processing);
            return RedirectToAction("Details", new { orderid = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartShip()
        {
            _orderService.ShipOrder(OrderVM.OrderHeader);
            return RedirectToAction("Details", new { orderid = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            _orderService.CancelOrder(OrderVM.OrderHeader.Id);
            return RedirectToAction("Details", new { orderid = OrderVM.OrderHeader.Id });
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                _orderService.DeleteOrder(id);
                return Json(new { success = true, message = "Order deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}
