
using TechStore.Entities.Models;
using TechStore.Entities.ViewModels;

namespace TechStore.Services.Interfaces
{
    public interface IOrderService
    {
        IEnumerable<OrderHeader> GetAllOrders();

        // FIX 6: GetUserOrders now returns properly paged/sorted results
        // so OrdersController no longer needs to query IUnitOfWork directly
        IEnumerable<OrderHeader> GetUserOrders(string userId);
        IEnumerable<OrderHeader> GetUserOrdersSorted(string userId);

        OrderVM GetOrderDetails(int orderId);
        void UpdateOrderDetails(OrderHeader orderHeader);
        void UpdateStatus(int orderId, string orderStatus, string? paymentStatus = null);
        void ShipOrder(OrderHeader orderHeader);
        void CancelOrder(int orderId);
        void DeleteOrder(int orderId);
    }
}
