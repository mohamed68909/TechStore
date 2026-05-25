using Microsoft.Extensions.Logging;
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

        // FIX 5: Inject ILogger so Stripe errors are visible in application logs
        private readonly ILogger<OrderService> _logger;

        public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IEnumerable<OrderHeader> GetAllOrders()
        {
            return _unitOfWork.OrderHeader.GetAll(includeWord: "ApplicationUser");
        }

        public OrderVM GetOrderDetails(int orderId)
        {
            return new OrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId, includeWord: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == orderId, includeWord: "Product")
            };
        }

        public void UpdateOrderDetails(OrderHeader orderHeader)
        {
            if (orderHeader == null) return;
            var orderInDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderHeader.Id);
            if (orderInDb == null) return;

            orderInDb.Name = orderHeader.Name;
            orderInDb.PhoneNumber = orderHeader.PhoneNumber;
            orderInDb.Address = orderHeader.Address;
            orderInDb.City = orderHeader.City;

            if (orderHeader.Carrier != null) orderInDb.Carrier = orderHeader.Carrier;
            if (orderHeader.TrackingNumber != null) orderInDb.TrackingNumber = orderHeader.TrackingNumber;

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
            if (orderHeader == null) return;
            var orderInDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderHeader.Id);
            if (orderInDb == null) return;

            orderInDb.TrackingNumber = orderHeader.TrackingNumber;
            orderInDb.Carrier = orderHeader.Carrier;
            orderInDb.OrderStatus = SD.Shipped;
            orderInDb.ShippingDate = DateTimeOffset.UtcNow;

            _unitOfWork.OrderHeader.Update(orderInDb);
            _unitOfWork.Complete();
        }

        public void CancelOrder(int orderId)
        {
            var orderInDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId);
            if (orderInDb == null) return;

            if (orderInDb.PaymentStatus == SD.Approve)
            {
                // FIX 5: Wrap Stripe refund in try/catch.
                // Previously the order was marked as refunded even if Stripe returned an error.
                // Now we only update the DB status AFTER the Stripe call succeeds.
                try
                {
                    var options = new RefundCreateOptions
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = orderInDb.PaymentIntentId
                    };
                    var service = new RefundService();
                    service.Create(options); // Throws StripeException on failure

                    // Only update to Refunded AFTER Stripe confirms the refund
                    _unitOfWork.OrderHeader.UpdateStatus(orderId, SD.Cancelled, SD.Refund);
                    _logger.LogInformation("Order {OrderId} refunded successfully via Stripe.", orderId);
                }
                catch (StripeException ex)
                {
                    // Log the Stripe error and rethrow so the controller can surface it to the admin
                    _logger.LogError(ex, "Stripe refund failed for Order {OrderId}: {Message}", orderId, ex.Message);
                    throw new InvalidOperationException(
                        $"Stripe refund failed: {ex.StripeError?.Message ?? ex.Message}", ex);
                }
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderId, SD.Cancelled, SD.Cancelled);
            }
            _unitOfWork.Complete();
        }

        public void DeleteOrder(int orderId)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId);
            if (orderHeader == null) return;

            var orderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId).ToList();
            _unitOfWork.OrderDetail.RemoveRange(orderDetails);
            _unitOfWork.OrderHeader.Remove(orderHeader);
            _unitOfWork.Complete();
        }

        public IEnumerable<OrderHeader> GetUserOrders(string userId)
        {
            // FIX 11 (H-5): Push ORDER BY to the database level via the orderBy parameter
            return _unitOfWork.OrderHeader.GetAll(
                u => u.ApplicationUserId == userId,
                orderBy: q => q.OrderByDescending(o => o.OrderDate));
        }

        // FIX 6: Used by the API OrdersController so it doesn't need IUnitOfWork
        public IEnumerable<OrderHeader> GetUserOrdersSorted(string userId)
            => GetUserOrders(userId);
    }
}
