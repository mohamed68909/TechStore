
using Microsoft.AspNetCore.Http;
using TechStore.Entities.Models;
using TechStore.Entities.ViewModels;

public interface IProductService
{
    IEnumerable<Product> GetAllProducts();
    ProductVM GetProductForCreate();
    ProductVM GetProductForEdit(int id);
    void AddProduct(ProductVM productVM, IFormFile? file, string webRootPath);
    void UpdateProduct(ProductVM productVM, IFormFile? file, string webRootPath);
    bool DeleteProduct(int id, string webRootPath);
}
