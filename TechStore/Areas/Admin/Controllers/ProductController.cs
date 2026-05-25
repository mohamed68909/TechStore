using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStore.Entities.ViewModels;
using TechStore.Utilities;

namespace TechStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult GetData()
        {
            var products = _productService.GetAllProducts();
            return Json(new { data = products });
        }

        [HttpGet]
        public IActionResult Create() => View(_productService.GetProductForCreate());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                using var fileStream = file?.OpenReadStream();
                string? fileName = file?.FileName;
                _productService.AddProduct(productVM, fileStream, fileName);
                TempData["Create"] = "Product has been added successfully";
                return RedirectToAction("Index");
            }
            return View(productVM);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();
            var vm = _productService.GetProductForEdit(id.Value);
            return vm.Product == null ? NotFound() : View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                using var fileStream = file?.OpenReadStream();
                string? fileName = file?.FileName;
                _productService.UpdateProduct(productVM, fileStream, fileName);
                TempData["Update"] = "Product has been updated successfully";
                return RedirectToAction("Index");
            }
            return View(productVM);
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var success = _productService.DeleteProduct(id);
            return Json(new { success = success, message = success ? "Product has been deleted successfully" : "Error while deleting" });
        }
    }
}
