using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using TechStore.Entities.Models;
using TechStore.Entities.Repositories;
using TechStore.Entities.ViewModels;
using TechStore.Services.Interfaces;
using TechStore.Utilities;

namespace TechStore.Services.Implementation
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CartService> _logger;

        public CartService(IUnitOfWork unitOfWork, ILogger<CartService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public ShoppingCartVM GetCartViewModel(string userId)
        {
            var vm = new ShoppingCartVM()
            {
                // FIX 11: renamed Includeword → includeWord, GetFirstorDefault → GetFirstOrDefault
                CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeWord: "Product"),
                OrderHeader = new OrderHeader()
            };
            CalculateTotalPrice(vm);
            return vm;
        }

        public ShoppingCartVM GetSummaryViewModel(string userId)
        {
            var vm = GetCartViewModel(userId);
            // FIX 11: renamed GetFirstorDefault → GetFirstOrDefault
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);

            if (user != null)
            {
                vm.OrderHeader.ApplicationUser = user;
                vm.OrderHeader.Name = user.Name ?? string.Empty;
                vm.OrderHeader.Address = user.Address ?? string.Empty;
                vm.OrderHeader.City = user.City ?? string.Empty;
                vm.OrderHeader.PhoneNumber = user.PhoneNumber ?? string.Empty;
            }
            else
            {
                vm.OrderHeader.Name = string.Empty;
                vm.OrderHeader.Address = string.Empty;
                vm.OrderHeader.City = string.Empty;
                vm.OrderHeader.PhoneNumber = string.Empty;
            }

            return vm;
        }

        // FIX 3: Wrap entire checkout flow in a DB transaction.
        // Before this fix, three separate Complete() calls meant a crash between saves
        // could leave a partially-created order (header saved but no SessionId) stuck forever.
        // Now the entire operation (OrderHeader + OrderDetails + SessionId) commits atomically.
        public async Task<Session> CreateStripeSessionAsync(
            ShoppingCartVM vm, string userId, string domain,
            string? successUrl = null, string? cancelUrl = null)
        {
            // Re-fetch cart from DB to ensure fresh, authoritative data with Product included
            vm.CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeWord: "Product");

            if (!vm.CartsList.Any())
                throw new InvalidOperationException("Cannot checkout with an empty cart.");

            // Begin an explicit EF Core database transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Step 1 — Persist OrderHeader
                vm.OrderHeader.OrderStatus = SD.Pending;
                vm.OrderHeader.PaymentStatus = SD.Pending;
                vm.OrderHeader.OrderDate = DateTimeOffset.UtcNow;
                vm.OrderHeader.ApplicationUserId = userId;
                CalculateTotalPrice(vm);

                _unitOfWork.OrderHeader.Add(vm.OrderHeader);
                await _unitOfWork.CompleteAsync(); // OrderHeader.Id is now set by EF

                // Step 2 — Persist OrderDetails
                foreach (var item in vm.CartsList)
                {
                    var detail = new OrderDetail
                    {
                        ProductId = item.ProductId,
                        OrderHeaderId = vm.OrderHeader.Id,
                        Price = item.Product.Price,
                        Count = item.Count
                    };
                    _unitOfWork.OrderDetail.Add(detail);
                }
                await _unitOfWork.CompleteAsync();

                // Step 3 — Create Stripe session (external call — outside DB but inside transaction scope)
                var finalSuccessUrl = BuildUrl(successUrl, vm.OrderHeader.Id, domain, "customer/cart/orderconfirmation");
                var finalCancelUrl = !string.IsNullOrEmpty(cancelUrl) ? cancelUrl : domain + "customer/cart/index";

                var options = new SessionCreateOptions
                {
                    LineItems = vm.CartsList.Select(item => new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Product.Price * 100),
                            Currency = SD.StripeCurrency,
                            ProductData = new SessionLineItemPriceDataProductDataOptions { Name = item.Product.Name }
                        },
                        Quantity = item.Count,
                    }).ToList(),
                    Mode = "payment",
                    SuccessUrl = finalSuccessUrl,
                    CancelUrl = finalCancelUrl,
                };

                Session session;
                try
                {
                    var sessionService = new SessionService();
                    session = sessionService.Create(options);
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, "Stripe session creation failed for user {UserId}: {Message}", userId, ex.Message);
                    throw new InvalidOperationException(
                        $"Payment gateway error: {ex.StripeError?.Message ?? ex.Message}", ex);
                }

                // Step 4 — Persist SessionId (final save before commit)
                vm.OrderHeader.SessionId = session.Id;
                await _unitOfWork.CompleteAsync();

                // All three saves succeeded — commit the transaction atomically
                await transaction.CommitAsync();

                _logger.LogInformation("Checkout completed. OrderId={OrderId}, SessionId={SessionId}", vm.OrderHeader.Id, session.Id);
                return session;
            }
            catch
            {
                // Transaction rolls back automatically on disposal when CommitAsync was not called,
                // but we log the event explicitly for observability.
                _logger.LogWarning("Checkout transaction rolled back for user {UserId}.", userId);
                throw;
            }
        }

        // Keep synchronous overload for backward compat with MVC CartController
        public Session CreateStripeSession(ShoppingCartVM vm, string userId, string domain,
            string? successUrl = null, string? cancelUrl = null)
        {
            return CreateStripeSessionAsync(vm, userId, domain, successUrl, cancelUrl)
                .GetAwaiter().GetResult();
        }

        public void ConfirmOrderPayment(int orderId, string userId)
        {
            // FIX 11: renamed GetFirstorDefault → GetFirstOrDefault
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId);
            if (orderHeader == null || orderHeader.ApplicationUserId != userId) return;

            var sessionId = orderHeader.SessionId;
            if (string.IsNullOrEmpty(sessionId)) return;

            var service = new SessionService();
            var session = service.Get(sessionId);

            if (session.PaymentStatus?.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderId, SD.Approve, SD.Approve);
                orderHeader.PaymentIntentId = session.PaymentIntentId;
                _unitOfWork.Complete();

                var carts = _unitOfWork.ShoppingCart
                    .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
                _unitOfWork.ShoppingCart.RemoveRange(carts);
                _unitOfWork.Complete();
            }
        }

        // FIX 6: These methods now fully encapsulate ownership validation inside the service.
        // Controllers no longer need to inject IUnitOfWork to do the pre-check.

        public int IncrementItem(int cartId, string userId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            if (cart == null || cart.ApplicationUserId != userId) return 0;
            _unitOfWork.ShoppingCart.IncreaseCount(cart, 1);
            _unitOfWork.Complete();
            return cart.Count;
        }

        public int DecrementItem(int cartId, string userId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            if (cart == null || cart.ApplicationUserId != userId) return 0;
            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Complete();
                return 0;
            }
            _unitOfWork.ShoppingCart.DecreaseCount(cart, 1);
            _unitOfWork.Complete();
            return cart.Count;
        }

        public int RemoveItem(int cartId, string userId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            if (cart == null || cart.ApplicationUserId != userId) return 0;
            var cartUserId = cart.ApplicationUserId;
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Complete();
            // FIX 9: Use Count() on the repo (no in-memory materialization)
            return _unitOfWork.ShoppingCart.Count(x => x.ApplicationUserId == cartUserId);
        }

        // FIX 6: New method to validate product exists before adding to cart.
        // Returns false when the product doesn't exist (prevents FK violation).
        public bool AddToCart(string userId, int productId, int count)
        {
            if (count <= 0) return false;

            var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == productId);
            if (product == null) return false;

            var existing = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                u => u.ApplicationUserId == userId && u.ProductId == productId);

            if (existing == null)
            {
                _unitOfWork.ShoppingCart.Add(new ShoppingCart
                {
                    ProductId = productId,
                    Count = count,
                    ApplicationUserId = userId
                });
            }
            else
            {
                _unitOfWork.ShoppingCart.IncreaseCount(existing, count);
            }
            _unitOfWork.Complete();
            return true;
        }

        private static void CalculateTotalPrice(ShoppingCartVM vm)
        {
            vm.OrderHeader.TotalPrice = 0; // Reset before summing to prevent double-counting
            foreach (var item in vm.CartsList)
            {
                vm.OrderHeader.TotalPrice += item.Count * item.Product.Price;
            }
        }

        private static string BuildUrl(string? customUrl, int orderId, string domain, string defaultPath)
        {
            if (string.IsNullOrEmpty(customUrl))
                return $"{domain}{defaultPath}?id={orderId}";

            return customUrl.Contains("{id}")
                ? customUrl.Replace("{id}", orderId.ToString())
                : customUrl + (customUrl.Contains('?') ? "&" : "?") + $"id={orderId}";
        }
    }
}
