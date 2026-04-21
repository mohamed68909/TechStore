using Microsoft.AspNetCore.Mvc;
using TechStore.Services.Interfaces;

namespace TechStore.Api.Controllers
{
    public class CategoriesController : BaseApiController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var categories = _categoryService.GetAllCategories();
            return Ok(new { success = true, data = categories });
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var category = _categoryService.GetCategoryById(id);
            if (category == null)
            {
                return NotFound(new { success = false, message = "Category not found" });
            }
            return Ok(new { success = true, data = category });
        }
    }
}
