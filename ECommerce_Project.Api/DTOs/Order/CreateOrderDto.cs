using ECommerce_Project.Api.DTOs.OrderItem;

namespace ECommerce_Project.Api.DTOs.Order
{
    public class CreateOrderDto
    {
        public string Address { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;

        public List<CreateOrderItemDto> Items { get; set; } = [];

    }
}
