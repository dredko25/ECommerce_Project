using System.ComponentModel.DataAnnotations;

namespace ECommerce_Project.Api.DTOs.Product
{
    public class UpdateProductDto
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Ціна має бути більшою за 0")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Кількість не може бути від'ємною")]
        public int? QuantityAvailable { get; set; }

        public string? ImageUrl { get; set; }

        public Guid? CategoryId { get; set; }

    }
}
