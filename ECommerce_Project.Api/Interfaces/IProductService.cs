using ECommerce_Project.Api.DTOs.Product;

namespace ECommerce_Project.Api.Interfaces
{
    public interface IProductService
    {
        //Task<IEnumerable<ProductSummaryDto>> GetProductsAsync();
        Task<List<ProductSummaryDto>> GetAllAsync();
        
        Task<List<ProductSummaryDto>> GetByCategoryAsync(Guid categoryId);
        
        Task<ProductResponseDto?> GetByIdAsync(Guid id);
        
        Task<ProductResponseDto> CreateAsync(CreateProductDto dto);
        
        Task<ProductResponseDto?> UpdateAsync(Guid id, UpdateProductDto dto);
        
        Task<bool> DeleteAsync(Guid id);
    }
}
