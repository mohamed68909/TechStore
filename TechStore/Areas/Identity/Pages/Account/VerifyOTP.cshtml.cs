using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TechStore.Entities.Models;
using TechStore.Services.Interfaces;

namespace TechStore.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class VerifyOTPModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IOTPService _otpService;

        public VerifyOTPModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOTPService otpService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _otpService = otpService;
        }

        [BindProperty]
        public string Email { get; set; } = default!;

        [BindProperty]
        [Required(ErrorMessage = "Please enter the activation code completely")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "The code must be exactly 6 digits")]
        public string OTP { get; set; } = default!;

        public void OnGet(string email)
        {
            Email = email;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                return RedirectToPage("./Login");
            }

            if (_otpService.ValidateOTP(user, OTP))
            {
                user.EmailConfirmed = true;
                user.OTPCode = null; // Clear OTP after success
                user.OTPExpiry = null;
                await _userManager.UpdateAsync(user);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToPage("/Index", new { area = "Customer" });
            }

            ModelState.AddModelError(string.Empty, "The activation code is incorrect or has expired.");
            return Page();
        }
    }
}
