namespace ECommerce_Project.Api.DTOs.User
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        
        public DateTime ExpiresAt { get; set; }
        
        public UserResponseDto User { get; set; } = null!;
    }
}
