
using TechStore.Entities.Models;
using TechStore.Entities.Repositories;
using TechStore.Services.Interfaces;
using X.PagedList;

namespace TechStore.Services.Implementation
{
    public class HomeService : IHomeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IPagedList<Product> GetProductsPaged(int pageNumber, int pageSize)
        {
            var totalCount = _unitOfWork.Product.Count();
            // FIX M-9: Pass orderBy so pagination is deterministic (ORDER BY in SQL)
            var products = _unitOfWork.Product.GetPaginated(
                pageNumber, pageSize,
                orderBy: q => q.OrderByDescending(p => p.Id));
            return new StaticPagedList<Product>(products, pageNumber, pageSize, totalCount);
        }

        // FIX 11: GetFirstorDefault → GetFirstOrDefault; Includeword → includeWord
        public ShoppingCart GetProductDetails(int productId)
        {
            return new ShoppingCart
            {
                ProductId = productId,
                Product = _unitOfWork.Product.GetFirstOrDefault(v => v.Id == productId, includeWord: "Category"),
                Count = 1
            };
        }

        // FIX M-10: Replaced magic -1 sentinel with always returning the current cart count.
        // Previous: returned -1 when updating, positive count when adding — undocumented and confusing.
        // Now: always returns current cart count so session can be kept accurate.
        public int AddOrUpdateCart(ShoppingCart shoppingCart)
        {
            var cartInDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                u => u.ApplicationUserId == shoppingCart.ApplicationUserId
                  && u.ProductId == shoppingCart.ProductId);

            if (cartInDb == null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            }
            else
            {
                _unitOfWork.ShoppingCart.IncreaseCount(cartInDb, shoppingCart.Count);
            }
            _unitOfWork.Complete();

            // FIX M-10 + FIX 9: Use Count() directly on the repo (no in-memory GetAll().Count())
            return _unitOfWork.ShoppingCart.Count(x => x.ApplicationUserId == shoppingCart.ApplicationUserId);
        }
    }
}
