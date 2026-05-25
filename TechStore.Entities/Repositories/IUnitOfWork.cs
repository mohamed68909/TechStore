
using Microsoft.EntityFrameworkCore.Storage;

namespace TechStore.Entities.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        IShoppingCartRepository ShoppingCart { get; }

        IOrderHeaderRepository OrderHeader { get; }
        IOrderDetailRepository OrderDetail { get; }

        IApplicationUserRepository ApplicationUser { get; }

        int Complete();
        Task<int> CompleteAsync();

        // FIX 3: Expose transaction support for multi-step operations like checkout
        // that must be atomic (all-or-nothing) to prevent partial data states.
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
