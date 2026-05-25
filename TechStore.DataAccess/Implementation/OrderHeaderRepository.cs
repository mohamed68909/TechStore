using TechStore.DataAccess.Data;
using TechStore.Entities.Models;
using TechStore.Entities.Repositories;

namespace TechStore.DataAccess.Implementation
{
    public class OrderHeaderRepository : GenericRepository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderHeader orderHeader)
        {
            _context.OrderHeaders.Update(orderHeader);
        }

        // FIX H-4: UpdateStatus previously always overwrote PaymentDate with UtcNow,
        // even when the update was purely for order status (e.g., "Processing", "Shipped").
        // Now PaymentDate is only updated when a paymentStatus is actually being changed.
        public void UpdateStatus(int id, string? orderStatus, string? paymentStatus)
        {
            var orderFromDb = _context.OrderHeaders.SingleOrDefault(x => x.Id == id);
            if (orderFromDb == null) return;

            if (orderStatus != null)
                orderFromDb.OrderStatus = orderStatus;

            // Only stamp PaymentDate when payment status is explicitly being updated
            if (paymentStatus != null)
            {
                orderFromDb.PaymentStatus = paymentStatus;
                orderFromDb.PaymentDate = DateTimeOffset.UtcNow;
            }
        }
    }
}
