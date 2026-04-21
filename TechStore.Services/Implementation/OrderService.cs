

using Stripe;
using TechStore.Entities.Models;
using TechStore.Entities.Repositories;
using TechStore.Entities.ViewModels;
using TechStore.Services.Interfaces;
using TechStore.Utilities;

namespace TechStore.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<OrderHeader> GetAllOrders()
        {
            return _unitOfWork.OrderHeader.GetAll(Includeword: "ApplicationUser");
        }

        public OrderVM GetOrderDetails(int orderId)
        {
            return new OrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstorDefault(u => u.Id == orderId, Includeword: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == orderId, Includeword: "Product")
            };
        }

        public void UpdateOrderDetails(OrderHeader orderHeader)
        {
            var orderInDb = _unitOfWork.OrderHeader.GetFirstorDefault(u => u.Id == orderHeader.Id);

            orderInDb.Name = orderHeader.Name;
            orderInDb.PhoneNumber = orderHeader.PhoneNumber;
            orderInDb.Address = orderHeader.Address;
            orderInDb.City = orderHeader.City;

            if (orderHeader.Carrier != null) orderInDb.Carrier = orderHeader.Carrier;
            if (orderHeader.TrakcingNumber != null) orderInDb.TrakcingNumber = orderHeader.TrakcingNumber;

            _unitOfWork.OrderHeader.Update(orderInDb);
            _unitOfWork.Complete();
        }

        public void UpdateStatus(int orderId, string orderStatus, string? paymentStatus = null)
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderId, orderStatus, paymentStatus);
            _unitOfWork.Complete();
        }

        public void ShipOrder(OrderHeader orderHeader)
        {
            var orderInDb = _unitOfWork.OrderHeader.GetFirstorDefault(u => u.Id == orderHeader.Id);
            orderInDb.TrakcingNumber = orderHeader.TrakcingNumber;
            orderInDb.Carrier = orderHeader.Carrier;
            orderInDb.OrderStatus = SD.Shipped;
            orderInDb.ShippingDate = DateTimeOffset.UtcNow;

            _unitOfWork.OrderHeader.Update(orderInDb);
            _unitOfWork.Complete();
        }

        public void CancelOrder(int orderId)
        {
            var orderInDb = _unitOfWork.OrderHeader.GetFirstorDefault(u => u.Id == orderId);

            if (orderInDb.PaymentStatus == SD.Approve)
            {
                // ???? ??????? ??????? ??? Stripe
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderInDb.PaymentIntentId
                };
                var service = new RefundService();
                service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderId, SD.Cancelled, SD.Refund);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderId, SD.Cancelled, SD.Cancelled);
            }
            _unitOfWork.Complete();
        }

        public IEnumerable<OrderHeader> GetUserOrders(string userId)
        {
            return _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId)
                                   .OrderByDescending(o => o.OrderDate);
        }
    }
}
