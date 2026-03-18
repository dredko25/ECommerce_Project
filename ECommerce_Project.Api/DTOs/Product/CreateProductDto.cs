using ECommerce_Project.DataAccess.Models;

namespace ECommerce_Project.Api.DTOs.Product
{
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; } = 0;

        public int QuantityAvailable { get; set; } = 0;

        public string? ImageUrl { get; set; } = string.Empty;

        public Guid? CategoryId { get; set; }
    }
}
