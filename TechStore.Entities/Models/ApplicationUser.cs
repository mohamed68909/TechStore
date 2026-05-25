


using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TechStore.Entities.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(6)]
        public string? OTPCode { get; set; }

        public DateTimeOffset? OTPExpiry { get; set; }

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
    }
}
