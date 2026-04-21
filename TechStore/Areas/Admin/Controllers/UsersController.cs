using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;
using TechStore.Services.Interfaces;
using TechStore.Utilities;

namespace TechStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            //  Claims
            var claimsIdentity = (ClaimsIdentity?)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            string userId = claim?.Value ?? string.Empty;

            var users = _userService.GetAllUsersExcept(userId);
            return View(users);
        }

        public IActionResult LockUnlock(string? id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var success = _userService.LockUnlockUser(id);
            if (!success) return NotFound();

            return RedirectToAction("Index", "Users", new { area = "Admin" });
        }
    }
}
