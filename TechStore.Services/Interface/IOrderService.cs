
using TechStore.Entities.Models;
using TechStore.Entities.ViewModels;

namespace TechStore.Services.Interfaces
{
    public interface IOrderService
    {
        IEnumerable<OrderHeader> GetAllOrders();
        OrderVM GetOrderDetails(int orderId);
        void UpdateOrderDetails(OrderHeader orderHeader);
        void UpdateStatus(int orderId, string orderStatus, string? paymentStatus = null);
        void ShipOrder(OrderHeader orderHeader);
        void CancelOrder(int orderId);

        IEnumerable<OrderHeader> GetUserOrders(string userId);
    }
}
