
using TechStore.Entities.Models;

namespace TechStore.Services.Interfaces
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetAllCategories();
        Category GetCategoryById(int id);
        void CreateCategory(Category category);
        void UpdateCategory(Category category);
        bool DeleteCategory(int id);
    }
}
