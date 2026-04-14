using TechStore.DataAccess.Data;
using TechStore.Entities.Models;
using TechStore.Entities.Repositories;

namespace TechStore.DataAccess.Implementation

{
    public class ShoppingCartRepository : GenericRepository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _context;
        public ShoppingCartRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public int DecreaseCount(ShoppingCart shoppingcart, int count)
        {
            shoppingcart.Count -= count;
            return shoppingcart.Count;
        }

        public int IncreaseCount(ShoppingCart shoppingcart, int count)
        {
            shoppingcart.Count += count;
            return shoppingcart.Count;
        }
    }
}
