namespace ECommerce_Project.DataAccess.Models;

public class CategoryEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;

    public List<ProductEntity> Products { get; set; } = [];

}
