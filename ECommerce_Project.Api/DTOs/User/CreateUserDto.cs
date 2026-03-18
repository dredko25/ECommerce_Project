using System.ComponentModel.DataAnnotations;

namespace ECommerce_Project.Api.DTOs.User
{
    public class CreateUserDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}
