namespace ECommerce_Project.Api.DTOs.OrderItem
{
    public class OrderItemResponseDto
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string? ProductImageUrl { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public decimal Total => Price * Quantity;

    }
}
