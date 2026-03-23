using ECommerce_Project.Api.DTOs.Cart;

namespace ECommerce_Project.Api.Interfaces
{
    public interface ICartService
    {
        Task<CartResponseDto?> GetByUserAsync(Guid userId);
        
        Task<CartResponseDto> AddItemAsync(Guid userId, AddToCartDto dto);
        
        Task<CartResponseDto?> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, int quantity);
        
        Task<CartResponseDto?> RemoveItemAsync(Guid userId, Guid cartItemId);
        
        Task ClearAsync(Guid userId);
    }
}
