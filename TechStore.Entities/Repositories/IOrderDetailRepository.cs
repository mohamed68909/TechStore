

using TechStore.Entities.Models;

namespace TechStore.Entities.Repositories
{
    public interface IOrderDetailRepository : IGenericRepository<OrderDetail>
    {
        void Update(OrderDetail orderDetail);
    }
}
