using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace TechStore.Entities.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }

        [MaxLength(450)]
        public string ApplicationUserId { get; set; } = string.Empty;

        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; } = default!;

        public DateTimeOffset OrderDate { get; set; }
        public DateTimeOffset ShippingDate { get; set; }

        public decimal TotalPrice { get; set; }

        [MaxLength(50)]
        public string? OrderStatus { get; set; }

        [MaxLength(50)]
        public string? PaymentStatus { get; set; }

        [MaxLength(100)]
        public string? TrackingNumber { get; set; }

        [MaxLength(100)]
        public string? Carrier { get; set; }

        public DateTimeOffset PaymentDate { get; set; }

        //Stripe Properties

        [MaxLength(500)]
        public string? SessionId { get; set; }

        [MaxLength(500)]
        public string? PaymentIntentId { get; set; }

        //User Data
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}
