using System;
using System.Collections.Generic;

namespace TechStore.Api.DTOs
{
    public class OrderHeaderDto
    {
        public int Id { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }

    public class OrderDetailDto
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Count { get; set; }
    }

    public class OrderFullDto
    {
        public OrderHeaderDto Header { get; set; } = new OrderHeaderDto();
        public List<OrderDetailDto> Details { get; set; } = new List<OrderDetailDto>();
    }
}
