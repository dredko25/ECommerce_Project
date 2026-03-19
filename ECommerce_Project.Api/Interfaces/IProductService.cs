using ECommerce_Project.Api.DTOs.Product;
using ECommerce_Project.Api.Helpers;

namespace ECommerce_Project.Api.Interfaces
{
    public interface IProductService
    {
        //Task<IEnumerable<ProductSummaryDto>> GetProductsAsync();
        //Task<List<ProductSummaryDto>> GetAllAsync();

        //Task<List<ProductSummaryDto>> GetByCategoryAsync(Guid categoryId);

        Task<PagedResponse<ProductSummaryDto>> GetProductsAsync(ProductParams productParams);

        Task<ProductResponseDto?> GetByIdAsync(Guid id);
        
        Task<ProductResponseDto> CreateAsync(CreateProductDto dto);
        
        Task<ProductResponseDto?> UpdateAsync(Guid id, UpdateProductDto dto);
        
        Task<bool> DeleteAsync(Guid id);
    }
}
