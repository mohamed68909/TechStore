using System;
using System.Threading.Tasks;
using TechStore.Entities.Models;

namespace TechStore.Services.Interfaces
{
    public interface IOTPService
    {
        string GenerateOTP();
        Task<bool> SendOTPAsync(string email, string otp);
        bool ValidateOTP(ApplicationUser user, string otp);
    }
}
