namespace ECommerce_Project.DataAccess.Models;

public class CartItemEntity
{
    public Guid Id { get; set; }

    public Guid CartId { get; set; }

    public CartEntity? Cart { get; set; }

    public Guid ProductId { get; set; }

    public ProductEntity? Product { get; set; }

    public int Quantity { get; set; } = 1;

}
