
using TechStore.Entities.Repositories;
using TechStore.Services.Interfaces;
using TechStore.Utilities;

namespace TechStore.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public int GetTotalOrdersCount() => _unitOfWork.OrderHeader.GetAll().Count();

        public int GetApprovedOrdersCount() => _unitOfWork.OrderHeader.GetAll(x => x.OrderStatus == SD.Approve).Count();

        public int GetTotalUsersCount() => _unitOfWork.ApplicationUser.GetAll().Count();

        public int GetTotalProductsCount() => _unitOfWork.Product.GetAll().Count();
    }
}
