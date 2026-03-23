using ECommerce_Project.Api.DTOs.Order;

namespace ECommerce_Project.Api.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderSummaryDto>> GetAllAsync();

        Task<List<OrderSummaryDto>> GetByUserAsync(Guid userId);

        Task<OrderResponseDto?> GetByIdAsync(Guid id);

        Task<OrderResponseDto> CreateAsync(Guid userId, CreateOrderDto dto);
        
        Task<OrderResponseDto?> UpdateStatusAsync(Guid id, string status);
        
        Task<bool> DeleteAsync(Guid id);
    }
}
