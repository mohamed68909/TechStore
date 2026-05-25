using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TechStore.Api.DTOs;
using TechStore.Entities.Models;
using TechStore.Services.Interfaces;
using TechStore.Utilities;

namespace TechStore.Api.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IOTPService _otpService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IOTPService otpService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _otpService = otpService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Email)) return BadRequest("Email is taken");

            var user = new ApplicationUser
            {
                UserName = registerDto.Email.ToLower(),
                Email = registerDto.Email.ToLower(),
                Name = registerDto.Name,
                Address = registerDto.Address,
                City = registerDto.City,
                PhoneNumber = registerDto.PhoneNumber,
                EmailConfirmed = false // Require OTP
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, SD.CustomerRole);

            // Generate OTP
            var otp = _otpService.GenerateOTP();
            user.OTPCode = otp;
            user.OTPExpiry = DateTimeOffset.UtcNow.AddMinutes(10);
            await _userManager.UpdateAsync(user);

            await _otpService.SendOTPAsync(user.Email, otp);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Name = user.Name ?? string.Empty,
                Token = _tokenService.CreateToken(user, new List<string> { SD.CustomerRole })
            };
        }

        // FIX 8: Rate limited to 5 requests/min per IP to prevent OTP brute-force
        // FIX 4: lockoutOnFailure must be enabled — configured in Identity options in Program.cs
        [HttpPost("login")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null) return Unauthorized("Invalid email");

            // FIX 4: lockoutOnFailure: true — failed attempts now count toward lockout
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

            if (!result.Succeeded) return Unauthorized("Invalid password");

            if (!user.EmailConfirmed)
            {
                return Unauthorized("Please verify your account using OTP.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Name = user.Name ?? string.Empty,
                Token = _tokenService.CreateToken(user, roles)
            };
        }

        // FIX 8: Rate limited to 5 requests/min per IP to prevent OTP enumeration
        [HttpPost("verify-otp")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult> VerifyOTP(VerifyOTPDto verifyDto)
        {
            var user = await _userManager.FindByEmailAsync(verifyDto.Email);
            if (user == null) return NotFound("User not found");

            if (_otpService.ValidateOTP(user, verifyDto.OTP))
            {
                user.EmailConfirmed = true;
                user.OTPCode = null;
                user.OTPExpiry = null;
                await _userManager.UpdateAsync(user);
                return Ok("Account verified successfully");
            }

            return BadRequest("Invalid or expired OTP");
        }

        private async Task<bool> UserExists(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email == email.ToLower());
        }
    }
}
