
using TechStore.Entities.Models;
using TechStore.Entities.Repositories;
using TechStore.Services.Interfaces;

namespace TechStore.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Category> GetAllCategories()
        {
            return _unitOfWork.Category.GetAll();
        }

        public Category GetCategoryById(int id)
        {
            return _unitOfWork.Category.GetFirstorDefault(x => x.Id == id);
        }

        public void CreateCategory(Category category)
        {
            _unitOfWork.Category.Add(category);
            _unitOfWork.Complete(); // ????? ??? ???? ??????
        }

        public void UpdateCategory(Category category)
        {
            _unitOfWork.Category.Update(category);
            _unitOfWork.Complete();
        }

        public bool DeleteCategory(int id)
        {
            var categoryInDb = _unitOfWork.Category.GetFirstorDefault(x => x.Id == id);
            if (categoryInDb != null)
            {
                _unitOfWork.Category.Remove(categoryInDb);
                _unitOfWork.Complete();
                return true;
            }
            return false;
        }
    }
}
