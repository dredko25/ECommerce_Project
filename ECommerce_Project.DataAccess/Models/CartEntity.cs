namespace ECommerce_Project.DataAccess.Models;

public class CartEntity
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid UserId { get; set; }

    public UserEntity? User { get; set; }

    public List<CartItemEntity> CartItems { get; set; } = [];

}
