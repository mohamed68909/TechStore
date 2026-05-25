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
            var claimsIdentity = (ClaimsIdentity?)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var users = _userService.GetAllUsersExcept(userId);
            return View(users);
        }

        // FIX 7: Changed from GET to POST with [ValidateAntiForgeryToken].
        // The previous GET implementation was vulnerable to CSRF — any website could
        // trigger a lock/unlock by embedding a simple <img src="/Admin/Users/LockUnlock?id=..."> tag.
        // A POST with anti-forgery token requires a legitimate form submission from our own pages.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LockUnlock(string? id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var success = _userService.LockUnlockUser(id);
            if (!success) return NotFound();

            return RedirectToAction(nameof(Index), "Users", new { area = "Admin" });
        }
    }
}
