using ECommerce_Project.Api.DTOs.Product;

namespace ECommerce_Project.Api.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductSummaryDto>> GetProductsAsync();
    }
}
