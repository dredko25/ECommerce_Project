namespace ECommerce_Project.DataAccess.Models;

public class OrderItemEntity
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public OrderEntity? Order { get; set; }

    public Guid ProductId { get; set; }

    public ProductEntity? Product { get; set; }

    public decimal Price { get; set; } = 0;

    public int Quantity { get; set; } = 0;

    public decimal Total => Price * Quantity;

}