using ECommerce_Project.Api.DTOs.User;

namespace ECommerce_Project.Api.Interfaces
{
    public interface IUserService
    {
        Task<List<UserResponseDto>> GetAllAsync();

        Task<UserResponseDto?> GetByIdAsync(Guid id);
        
        Task<AuthResponseDto> CreateAsync(CreateUserDto dto);

        Task<AuthResponseDto> LoginAsync(LoginUserDto dto);

        Task<UserResponseDto?> UpdateAsync(Guid id, UpdateUserDto dto);
        
        Task<bool> DeleteAsync(Guid id);
    }
}
