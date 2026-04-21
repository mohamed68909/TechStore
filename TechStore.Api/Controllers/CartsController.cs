using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using TechStore.Api.DTOs;
using TechStore.Entities.Models;
using TechStore.Entities.Repositories;
using TechStore.Services.Interfaces;

namespace TechStore.Api.Controllers
{
    [Authorize]
    public class CartsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICartService _cartService;

        public CartsController(IUnitOfWork unitOfWork, ICartService cartService)
        {
            _unitOfWork = unitOfWork;
            _cartService = cartService;
        }

        [HttpGet]
        public ActionResult<CartDto> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartVM = _cartService.GetCartViewModel(userId!);

            var response = new CartDto
            {
                TotalPrice = cartVM.OrderHeader.TotalPrice,
                Items = cartVM.CartsList.Select(c => new CartItemDto
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    ProductPrice = c.Product.Price,
                    ProductImg = c.Product.Img,
                    Count = c.Count
                }).ToList()
            };

            return response;
        }

        [HttpPost("add")]
        public ActionResult AddToCart(int productId, int count = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var cartFromDb = _unitOfWork.ShoppingCart.GetFirstorDefault(
                u => u.ApplicationUserId == userId && u.ProductId == productId);

            if (cartFromDb == null)
            {
                var cart = new ShoppingCart
                {
                    ProductId = productId,
                    Count = count,
                    ApplicationUserId = userId!
                };
                _unitOfWork.ShoppingCart.Add(cart);
            }
            else
            {
                _unitOfWork.ShoppingCart.IncreaseCount(cartFromDb, count);
            }

            _unitOfWork.Complete();
            return Ok("Product added to cart");
        }

        [HttpPatch("increment/{cartId}")]
        public ActionResult<int> Increment(int cartId)
        {
            return _cartService.IncrementItem(cartId);
        }

        [HttpPatch("decrement/{cartId}")]
        public ActionResult<int> Decrement(int cartId)
        {
            return _cartService.DecrementItem(cartId);
        }

        [HttpDelete("{cartId}")]
        public ActionResult Remove(int cartId)
        {
            _cartService.RemoveItem(cartId);
            return NoContent();
        }

        [HttpPost("checkout")]
        public ActionResult<PaymentResponseDto> Checkout(CheckoutDto checkoutDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartVM = _cartService.GetCartViewModel(userId!);

            if (!cartVM.CartsList.Any())
            {
                return BadRequest("Cart is empty");
            }

            // Fill header info from DTO
            cartVM.OrderHeader.Name = checkoutDto.Name;
            cartVM.OrderHeader.Address = checkoutDto.Address;
            cartVM.OrderHeader.City = checkoutDto.City;
            cartVM.OrderHeader.PhoneNumber = checkoutDto.PhoneNumber;

            // Use provided URLs or defaults
            var domain = Request.Scheme + "://" + Request.Host + "/";
            var session = _cartService.CreateStripeSession(cartVM, userId!, domain);

            return Ok(new PaymentResponseDto
            {
                SessionId = session.Id,
                PaymentUrl = session.Url,
                OrderId = cartVM.OrderHeader.Id
            });
        }
    }
}
