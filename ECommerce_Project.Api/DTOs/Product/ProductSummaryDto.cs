namespace ECommerce_Project.Api.DTOs.Product
{
    public class ProductSummaryDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string? CategoryName { get; set; }

        public bool IsAvailable => QuantityAvailable > 0;

        public int QuantityAvailable { get; set; }

    }
}
