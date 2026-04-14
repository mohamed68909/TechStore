using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechStore.DataAccess.Data;
using TechStore.Entities.Models;
using TechStore.Utilities;

namespace TechStore.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;


        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;


        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(
            UserManager<ApplicationUser> userManager,


            RoleManager<IdentityRole> roleManager
            , ApplicationDbContext context
            )
        {
            _userManager = userManager;


            _roleManager = roleManager;
            _context = context;
        }

        public void Initialize()
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Count() > 0)
                {
                    _context.Database.Migrate();
                }

            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"An error occurred during database initialization: {ex.Message}");
            }


            if (!_roleManager.RoleExistsAsync(SD.AdminRole).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.AdminRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.EditorRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.CustomerRole)).GetAwaiter().GetResult();
            }
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "Mohamed",
                Email = "Admin@gmail.com",
                Name = "Mohamed",
                City = "Cairo",
                Address = "Cairo"
            }, "Admin123*").GetAwaiter().GetResult();
            ApplicationUser user = _context.ApplicationUsers.FirstOrDefault(u => u.Email == "Admin@gmail.com");
            _userManager.AddToRoleAsync(user, SD.AdminRole).GetAwaiter().GetResult();

        }
    }
}
