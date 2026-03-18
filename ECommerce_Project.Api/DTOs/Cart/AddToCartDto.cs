namespace ECommerce_Project.Api.DTOs.Cart
{
    public class AddToCartDto
    {
        public Guid ProductId { get; set; }

        public int Quantity { get; set; } = 1;
    }
}
