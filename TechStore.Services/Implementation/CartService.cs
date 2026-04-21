
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

        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ShoppingCartVM GetCartViewModel(string userId)
        {
            var vm = new ShoppingCartVM()
            {
                CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, Includeword: "Product"),
                OrderHeader = new OrderHeader()
            };
            CalculateTotalPrice(vm);
            return vm;
        }

        public ShoppingCartVM GetSummaryViewModel(string userId)
        {
            var vm = GetCartViewModel(userId);
            var user = _unitOfWork.ApplicationUser.GetFirstorDefault(x => x.Id == userId);

            vm.OrderHeader.ApplicationUser = user;
            vm.OrderHeader.Name = user.Name;
            vm.OrderHeader.Address = user.Address;
            vm.OrderHeader.City = user.City;
            vm.OrderHeader.PhoneNumber = user.PhoneNumber;

            return vm;
        }

        public Session CreateStripeSession(ShoppingCartVM vm, string userId, string domain)
        {
            vm.CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, Includeword: "Product");

            // 1. ????? ??? ?????
            vm.OrderHeader.OrderStatus = SD.Pending;
            vm.OrderHeader.PaymentStatus = SD.Pending;
            vm.OrderHeader.OrderDate = DateTimeOffset.UtcNow;
            vm.OrderHeader.ApplicationUserId = userId;
            CalculateTotalPrice(vm);

            _unitOfWork.OrderHeader.Add(vm.OrderHeader);
            _unitOfWork.Complete();

            // 2. ????? ?????? ?????
            foreach (var item in vm.CartsList)
            {
                var detail = new OrderDetail()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = vm.OrderHeader.Id,
                    Price = item.Product.Price,
                    Count = item.Count
                };
                _unitOfWork.OrderDetail.Add(detail);
            }
            _unitOfWork.Complete();

            // 3. ????? Stripe
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"customer/cart/orderconfirmation?id={vm.OrderHeader.Id}",
                CancelUrl = domain + $"customer/cart/index",
            };

            foreach (var item in vm.CartsList)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions { Name = item.Product.Name }
                    },
                    Quantity = item.Count,
                });
            }

            var service = new SessionService();
            Session session = service.Create(options);

            vm.OrderHeader.SessionId = session.Id;
            _unitOfWork.Complete();

            return session;
        }

        public void ConfirmOrderPayment(int orderId, string sessionId)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetFirstorDefault(u => u.Id == orderId);
            var service = new SessionService();
            Session session = service.Get(sessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderId, SD.Approve, SD.Approve);
                orderHeader.PaymentIntentId = session.PaymentIntentId;
                _unitOfWork.Complete();
            }

            var carts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(carts);
            _unitOfWork.Complete();
        }

        public int IncrementItem(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstorDefault(x => x.Id == cartId);
            _unitOfWork.ShoppingCart.IncreaseCount(cart, 1);
            _unitOfWork.Complete();
            return cart.Count;
        }

        public int DecrementItem(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstorDefault(x => x.Id == cartId);
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

        public int RemoveItem(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstorDefault(x => x.Id == cartId);
            var userId = cart.ApplicationUserId;
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Complete();
            return _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId).Count();
        }

        private void CalculateTotalPrice(ShoppingCartVM vm)
        {
            foreach (var item in vm.CartsList)
            {
                vm.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }
        }
    }
}
