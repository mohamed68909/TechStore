using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechStore.Api.DTOs;
using TechStore.Services.Interfaces;

namespace TechStore.Api.Controllers
{
    // FIX 6: Removed IUnitOfWork injection. All data access now flows through IOrderService only.
    // Previously the controller queried _unitOfWork.OrderHeader.GetAll() directly, bypassing the service layer.
    [Authorize]
    public class OrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        [HttpGet]
        public ActionResult<IEnumerable<OrderHeaderDto>> GetOrders()
        {
            // FIX 6: Uses service method (which sorts at DB level) — no direct repo access
            var orders = _orderService.GetUserOrdersSorted(GetUserId());

            return Ok(orders.Select(o => new OrderHeaderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalPrice = o.TotalPrice,
                OrderStatus = o.OrderStatus ?? "Pending",
                PaymentStatus = o.PaymentStatus ?? "Pending",
                Name = o.Name,
                City = o.City
            }));
        }

        [HttpGet("{id}")]
        public ActionResult<OrderFullDto> GetOrderDetails(int id)
        {
            var userId = GetUserId();
            var orderVM = _orderService.GetOrderDetails(id);

            if (orderVM == null || orderVM.OrderHeader == null)
                return NotFound("Order not found.");

            // Ownership check: customers can only view their own orders
            if (orderVM.OrderHeader.ApplicationUserId != userId)
                return Unauthorized("You are not authorized to view this order.");

            return Ok(new OrderFullDto
            {
                Header = new OrderHeaderDto
                {
                    Id = orderVM.OrderHeader.Id,
                    OrderDate = orderVM.OrderHeader.OrderDate,
                    TotalPrice = orderVM.OrderHeader.TotalPrice,
                    OrderStatus = orderVM.OrderHeader.OrderStatus ?? "Pending",
                    PaymentStatus = orderVM.OrderHeader.PaymentStatus ?? "Pending",
                    Name = orderVM.OrderHeader.Name,
                    City = orderVM.OrderHeader.City
                },
                Details = orderVM.OrderDetails.Select(d => new OrderDetailDto
                {
                    ProductName = d.Product.Name,
                    Price = d.Price,
                    Count = d.Count
                }).ToList()
            });
        }
    }
}
