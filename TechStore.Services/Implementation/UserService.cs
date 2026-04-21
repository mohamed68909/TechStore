
using TechStore.Entities.Models;
using TechStore.Entities.Repositories;
using TechStore.Services.Interfaces;

namespace TechStore.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<ApplicationUser> GetAllUsersExcept(string currentUserId)
        {
           
            return _unitOfWork.ApplicationUser.GetAll(x => x.Id != currentUserId);
        }

        public bool LockUnlockUser(string userId)
        {
            var user = _unitOfWork.ApplicationUser.GetFirstorDefault(x => x.Id == userId);
            if (user == null) return false;

          
            if (user.LockoutEnd == null || user.LockoutEnd < DateTimeOffset.UtcNow)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(1);
            }
            else 
            {
                user.LockoutEnd = DateTimeOffset.UtcNow;
            }

            _unitOfWork.Complete();
            return true;
        }
    }
}
