
using TechStore.Entities.Models;
using TechStore.Entities.ViewModels;

public interface IProductService
{
    IEnumerable<Product> GetAllProducts();
    Product? GetProductById(int id);
    IEnumerable<Product> GetProductsByCategoryId(int categoryId);
    ProductVM GetProductForCreate();
    ProductVM GetProductForEdit(int id);
    void AddProduct(ProductVM productVM, Stream? fileStream, string? fileName);
    void UpdateProduct(ProductVM productVM, Stream? fileStream, string? fileName);
    bool DeleteProduct(int id);
}
