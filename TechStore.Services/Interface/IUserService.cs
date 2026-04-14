

using TechStore.Entities.Models;

namespace TechStore.Services.Interfaces
{
    public interface IUserService
    {
        IEnumerable<ApplicationUser> GetAllUsersExcept(string currentUserId);
        bool LockUnlockUser(string userId);
    }
}
