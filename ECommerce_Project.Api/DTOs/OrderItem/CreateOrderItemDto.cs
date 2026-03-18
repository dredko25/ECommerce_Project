namespace ECommerce_Project.Api.DTOs.OrderItem
{
    public class CreateOrderItemDto
    {
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
