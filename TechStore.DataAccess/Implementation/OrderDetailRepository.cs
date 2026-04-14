using TechStore.DataAccess.Data;
using TechStore.Entities.Models;
using TechStore.Entities.Repositories;

namespace TechStore.DataAccess.Implementation

{
    public class OrderDetailRepository : GenericRepository<OrderDetail>, IOrderDetailRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderDetailRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderDetail Orderdetail)
        {
            _context.OrderDetails.Update(Orderdetail);
        }
    }
}
