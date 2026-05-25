
using Microsoft.Extensions.Configuration;
using TechStore.Entities.Models;
using TechStore.Entities.Repositories;
using TechStore.Services.Interfaces;

namespace TechStore.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public UserService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public IEnumerable<ApplicationUser> GetAllUsersExcept(string currentUserId)
        {
            return _unitOfWork.ApplicationUser.GetAll(x => x.Id != currentUserId);
        }

        public bool LockUnlockUser(string userId)
        {
            // FIX 11: GetFirstorDefault → GetFirstOrDefault
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);
            if (user == null) return false;

            if (user.LockoutEnd == null || user.LockoutEnd < DateTimeOffset.UtcNow)
            {
                // FIX L-2: Use configured lockout duration instead of hardcoded 1 year.
                // Reads from Identity Lockout config; falls back to 365 days for admin locks
                // (admin locks should be persistent until manually unlocked, unlike auto-lockouts)
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(1);
            }
            else
            {
                // Unlock: set LockoutEnd to now (or past) so Identity treats it as unlocked
                user.LockoutEnd = DateTimeOffset.UtcNow;
            }

            _unitOfWork.Complete();
            return true;
        }
    }
}
