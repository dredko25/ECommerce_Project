using ECommerce_Project.Api.DTOs.OrderItem;

namespace ECommerce_Project.Api.DTOs.Order
{
    public class OrderResponseDto
    {
        public Guid Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string UserFullName { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public List<OrderItemResponseDto> OrderItems { get; set; } = [];

    }
}
