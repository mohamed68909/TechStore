using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TechStore.Api.DTOs;
using TechStore.Entities.Repositories;
using TechStore.Services.Interfaces;

namespace TechStore.Api.Controllers
{
    [Authorize]
    public class OrdersController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderService _orderService;

        public OrdersController(IUnitOfWork unitOfWork, IOrderService orderService)
        {
            _unitOfWork = unitOfWork;
            _orderService = orderService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<OrderHeaderDto>> GetOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId)
                .OrderByDescending(u => u.OrderDate);

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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orderVM = _orderService.GetOrderDetails(id);

            if (orderVM.OrderHeader.ApplicationUserId != userId)
            {
                return Unauthorized("You are not authorized to view this order.");
            }

            var response = new OrderFullDto
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
            };

            return Ok(response);
        }
    }
}
