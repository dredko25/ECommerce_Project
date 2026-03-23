using ECommerce_Project.Api.DTOs.Category;

namespace ECommerce_Project.Api.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryResponseDto>> GetAllAsync();
        
        Task<CategoryResponseDto?> GetByIdAsync(Guid id);
        
        Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto);
        
        Task<CategoryResponseDto?> UpdateAsync(Guid id, UpdateCategoryDto dto);
        
        Task<bool> DeleteAsync(Guid id);
    }
}
