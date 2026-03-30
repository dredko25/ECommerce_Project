using ECommerce_Project.DataAccess.Models;

namespace ECommerce_Project.Api.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(UserEntity user);

        Task<string> GenerateAndSaveRefreshTokenAsync(UserEntity user);

        Task<UserEntity?> ValidateRefreshTokenAsync(Guid userId, string refreshToken);
    }
}