using ECommerce_Project.DataAccess.Models;

namespace ECommerce_Project.Api.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(UserEntity user);
    }
}