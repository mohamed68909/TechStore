
using TechStore.Entities.Models;

namespace TechStore.Services.Interfaces
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetAllCategories();
        // FIX 11/H-3: Return type is now Category? to eliminate unsafe null-forgiving ! in callers
        Category? GetCategoryById(int id);
        void CreateCategory(Category category);
        void UpdateCategory(Category category);
        bool DeleteCategory(int id);
    }
}
