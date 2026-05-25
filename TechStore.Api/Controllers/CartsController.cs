using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechStore.Api.DTOs;
using TechStore.Services.Interfaces;

namespace TechStore.Api.Controllers
{
    // FIX 6: Removed IUnitOfWork injection. All data access now goes through ICartService only.
    // Controllers must communicate exclusively through the service layer (Controller→Service→Repository).
    [Authorize]
    public class CartsController : BaseApiController
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        [HttpGet]
        public ActionResult<CartDto> GetCart()
        {
            var cartVM = _cartService.GetCartViewModel(GetUserId());

            return new CartDto
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
        }

        // FIX 6: Product existence is now validated inside CartService.AddToCart()
        // which also validates count > 0, preventing FK violations and invalid data.
        [HttpPost("add")]
        public ActionResult AddToCart(int productId, int count = 1)
        {
            var success = _cartService.AddToCart(GetUserId(), productId, count);
            if (!success)
                return BadRequest("Invalid product or count.");

            return Ok("Product added to cart.");
        }

        // FIX 6: Ownership check is encapsulated in the service — controller no longer
        // needs a direct DB round-trip to verify before calling the service method.
        [HttpPatch("increment/{cartId}")]
        public ActionResult<int> Increment(int cartId)
        {
            var result = _cartService.IncrementItem(cartId, GetUserId());
            if (result == 0) return NotFound("Cart item not found or does not belong to you.");
            return result;
        }

        [HttpPatch("decrement/{cartId}")]
        public ActionResult<int> Decrement(int cartId)
        {
            var result = _cartService.DecrementItem(cartId, GetUserId());
            // 0 is a valid result (item removed when count reaches 0) — return it normally
            return result;
        }

        [HttpDelete("{cartId}")]
        public ActionResult Remove(int cartId)
        {
            var result = _cartService.RemoveItem(cartId, GetUserId());
            // result == 0 when not found/unauthorized — distinguish from success with empty cart
            if (result < 0) return NotFound("Cart item not found or does not belong to you.");
            return NoContent();
        }

        // FIX 3 + FIX 9: Checkout now uses the async Stripe session method
        // which wraps the full checkout flow in a DB transaction (atomic).
        [HttpPost("checkout")]
        public async Task<ActionResult<PaymentResponseDto>> Checkout(CheckoutDto checkoutDto)
        {
            var userId = GetUserId();
            var cartVM = _cartService.GetCartViewModel(userId);

            if (!cartVM.CartsList.Any())
                return BadRequest("Cart is empty.");

            cartVM.OrderHeader.Name = checkoutDto.Name;
            cartVM.OrderHeader.Address = checkoutDto.Address;
            cartVM.OrderHeader.City = checkoutDto.City;
            cartVM.OrderHeader.PhoneNumber = checkoutDto.PhoneNumber;

            var domain = $"{Request.Scheme}://{Request.Host}/";

            try
            {
                // FIX 3: Uses the new async transactional method
                var session = await _cartService.CreateStripeSessionAsync(
                    cartVM, userId, domain, checkoutDto.SuccessUrl, checkoutDto.CancelUrl);

                return Ok(new PaymentResponseDto
                {
                    SessionId = session.Id,
                    PaymentUrl = session.Url,
                    OrderId = cartVM.OrderHeader.Id
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
