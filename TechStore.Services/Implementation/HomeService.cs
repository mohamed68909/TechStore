


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
            
            return _unitOfWork.Product.GetAll().ToPagedList(pageNumber, pageSize);
        }

        public ShoppingCart GetProductDetails(int productId)
        {
            return new ShoppingCart()
            {
                ProductId = productId,
                Product = _unitOfWork.Product.GetFirstorDefault(v => v.Id == productId, Includeword: "Category"),
                Count = 1
            };
        }

        public int AddOrUpdateCart(ShoppingCart shoppingCart)
        {
  
            ShoppingCart cartInDb = _unitOfWork.ShoppingCart.GetFirstorDefault(
                u => u.ApplicationUserId == shoppingCart.ApplicationUserId && u.ProductId == shoppingCart.ProductId);

            if (cartInDb == null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Complete();
                //  Session
                return _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingCart.ApplicationUserId).Count();
            }
            else
            {
                _unitOfWork.ShoppingCart.IncreaseCount(cartInDb, shoppingCart.Count);
                _unitOfWork.Complete();
                return -1; 
            }
        }
    }
}
