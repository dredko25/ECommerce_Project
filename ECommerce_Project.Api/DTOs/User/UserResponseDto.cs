namespace ECommerce_Project.Api.DTOs.User
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsAdmin { get; set; } = false;
    }
}
