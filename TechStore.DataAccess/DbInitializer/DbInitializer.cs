using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechStore.DataAccess.Data;
using TechStore.Entities.Models;
using TechStore.Utilities;

namespace TechStore.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public DbInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await _context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during database initialization: {ex.Message}");
                return;
            }

            if (!await _roleManager.RoleExistsAsync(SD.AdminRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.AdminRole));
                await _roleManager.CreateAsync(new IdentityRole(SD.EditorRole));
                await _roleManager.CreateAsync(new IdentityRole(SD.CustomerRole));
            }

            var adminEmail = _configuration["AdminSettings:Email"] ?? "Admin@gmail.com";
            var adminPassword = _configuration["AdminSettings:Password"] ?? "Admin123*";
            var adminName = _configuration["AdminSettings:Name"] ?? "Admin";
            var adminCity = _configuration["AdminSettings:City"] ?? "Cairo";
            var adminAddress = _configuration["AdminSettings:Address"] ?? "Cairo";

            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminName,
                    Email = adminEmail,
                    Name = adminName,
                    City = adminCity,
                    Address = adminAddress,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(newAdmin, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newAdmin, SD.AdminRole);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    Console.WriteLine($"Failed to create seed admin user: {errors}");
                }
            }
            else
            {
                if (!existingAdmin.EmailConfirmed)
                {
                    existingAdmin.EmailConfirmed = true;
                    await _userManager.UpdateAsync(existingAdmin);
                }
                if (!await _userManager.IsInRoleAsync(existingAdmin, SD.AdminRole))
                {
                    await _userManager.AddToRoleAsync(existingAdmin, SD.AdminRole);
                }
            }
        }
    }
}
