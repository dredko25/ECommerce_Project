namespace ECommerce_Project.DataAccess.Models;

public class UserEntity
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string ContactNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsAdmin { get; set; } = false;

    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public Guid? CartId { get; set; }

    public CartEntity? Cart { get; set; }

    public List<OrderEntity> Orders { get; set; } = [];

}
