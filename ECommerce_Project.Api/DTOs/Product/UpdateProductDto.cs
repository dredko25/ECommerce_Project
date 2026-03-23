namespace ECommerce_Project.Api.DTOs.Product
{
    public class UpdateProductDto
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal? Price { get; set; }

        public int? QuantityAvailable { get; set; }

        public string? ImageUrl { get; set; }

        public Guid? CategoryId { get; set; }

    }
}
