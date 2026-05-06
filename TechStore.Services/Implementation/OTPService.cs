using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Threading.Tasks;
using TechStore.Entities.Models;
using TechStore.Services.Interfaces;

namespace TechStore.Services.Implementation
{
    public class OTPService : IOTPService
    {
        private readonly IEmailSender _emailSender;

        public OTPService(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public string GenerateOTP()
        {
            return Random.Shared.Next(100000, 999999).ToString();
        }

        public async Task<bool> SendOTPAsync(string email, string otp)
        {
            try
            {
                await _emailSender.SendEmailAsync(email, "TechStore - Verification Code", 
                    $"Your verification code is: <b>{otp}</b>. This code will expire in 10 minutes.");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidateOTP(ApplicationUser user, string otp)
        {
            if (user.OTPCode == otp && user.OTPExpiry > DateTimeOffset.UtcNow)
            {
                return true;
            }
            return false;
        }
    }
}
