
using TechStore.Entities.Models;
using X.PagedList;

namespace TechStore.Services.Interfaces
{
    public interface IHomeService
    {
        IPagedList<Product> GetProductsPaged(int pageNumber, int pageSize);
        ShoppingCart GetProductDetails(int productId);
        int AddOrUpdateCart(ShoppingCart shoppingCart);
    }
}
