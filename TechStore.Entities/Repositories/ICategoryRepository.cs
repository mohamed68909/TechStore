

using TechStore.Entities.Models;

namespace TechStore.Entities.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        void Update(Category category);
    }
}
