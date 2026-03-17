namespace ECommerce_Project.DataAccess.Models;

public class ProductEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; } = 0;

    public int QuantityAvailable { get; set; } = 0;

    public string? ImageUrl { get; set; } = string.Empty;

    public Guid? CategoryId { get; set; }

    public CategoryEntity? Category { get; set; }
}
