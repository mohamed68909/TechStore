using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TechStore.Entities.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;

        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; } = default!;

        public DateTimeOffset OrderDate { get; set; }
        public DateTimeOffset ShippingDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }

        public string? TrakcingNumber { get; set; }
        public string? Carrier { get; set; }

        public DateTimeOffset PaymentDate { get; set; }

        //Stripe Properties

        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }

        //User Data
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

    }
}
