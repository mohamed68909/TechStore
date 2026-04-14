

using TechStore.Entities.Models;

namespace TechStore.Entities.Repositories
{
    public interface IOrderHeaderRepository : IGenericRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
        void UpdateStatus(int id, string? OrderStatus, string? PaymentStatus);
    }
}
