namespace ECommerce_Project.Api.DTOs.User
{
    public class TokenResponseDto
    {
        public required string AccessToken { get; set; }
        
        public required string RefreshToken { get; set; }
    }
}
