using ECommerce_Project.DataAccess.Models;
using System.ComponentModel.DataAnnotations;

namespace ECommerce_Project.Api.DTOs.Product
{
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Ціна має бути більшою за 0")]
        public decimal Price { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Кількість не може бути від'ємною")]
        public int QuantityAvailable { get; set; } = 0;

        public string? ImageUrl { get; set; } = string.Empty;

        public Guid? CategoryId { get; set; }
    }
}
