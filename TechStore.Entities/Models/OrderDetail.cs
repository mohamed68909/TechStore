using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TechStore.Entities.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderHeaderId { get; set; }
        [ValidateNever]
        public OrderHeader OrderHeader { get; set; } = default!;

        public int ProductId { get; set; }
        [ValidateNever]
        public Product Product { get; set; } = default!;

        public decimal Price { get; set; }

        public int Count { get; set; }


    }
}
