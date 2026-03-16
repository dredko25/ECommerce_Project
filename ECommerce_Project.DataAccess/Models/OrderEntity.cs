namespace ECommerce_Project.DataAccess.Models;

public class OrderEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public UserEntity? User { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public enum OrderStatus { Pending, Paid, Shipped, Delivered, Cancelled }

    public string PaymentMethod { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; } = 0;

    public List<OrderItemEntity> OrderItems { get; set; } = [];
}
