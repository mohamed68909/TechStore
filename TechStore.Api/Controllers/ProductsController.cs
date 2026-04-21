using Microsoft.AspNetCore.Mvc;
using TechStore.Services.Interfaces;

namespace TechStore.Api.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var products = _productService.GetAllProducts();
            return Ok(new { success = true, data = products });
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var product = _productService.GetAllProducts().FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound(new { success = false, message = "Product not found" });
            }
            return Ok(new { success = true, data = product });
        }

        [HttpGet("category/{categoryId}")]
        public IActionResult GetByCategory(int categoryId)
        {
            var products = _productService.GetAllProducts().Where(p => p.CategoryId == categoryId);
            return Ok(new { success = true, data = products });
        }
    }
}
