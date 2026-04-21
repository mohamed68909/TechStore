


using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TechStore.Entities.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? OTPCode { get; set; }
        public DateTimeOffset? OTPExpiry { get; set; }
        public string City { get; set; } = string.Empty;
    }
}
