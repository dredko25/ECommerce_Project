using ECommerce_Project.Api.DTOs.CartItem;

namespace ECommerce_Project.Api.DTOs.Cart
{
    public class CartResponseDto
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<CartItemResponseDto> Items { get; set; } = [];

        public decimal TotalAmount => Items.Sum(i => i.Total);

        public int TotalItems => Items.Sum(i => i.Quantity);

    }
}
