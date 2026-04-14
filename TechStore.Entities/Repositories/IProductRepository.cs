
using TechStore.Entities.Models;

namespace TechStore.Entities.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        void Update(Product product);
    }
}
