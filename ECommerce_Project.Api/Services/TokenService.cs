using ECommerce_Project.Api.Interfaces;
using ECommerce_Project.DataAccess;
using ECommerce_Project.DataAccess.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ECommerce_Project.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ECommerceDbContext _context;

        public TokenService(IConfiguration configuration, ECommerceDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// Generates a JSON Web Token (JWT) access token containing user identity and role claims.
        /// </summary>
        /// <remarks>The generated token includes claims for the user's identifier, email, first name,
        /// last name, and role. The token is valid for one day from the time of generation. Ensure that the application
        /// configuration contains valid JWT settings for key, issuer, and audience.</remarks>
        /// <param name="user">The user entity for which to generate the access token. Must not be null and must contain valid user
        /// information.</param>
        /// <returns>A JWT access token as a string, representing the authenticated user's identity and role claims.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the JWT signing key is not configured in the application settings.</exception>
        public string GenerateAccessToken(UserEntity user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "Customer")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("JWT Key is not configured")));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validates the specified refresh token for the given user and returns the user entity if the token is valid.
        /// </summary>
        /// <remarks>The method checks that the user exists, the provided refresh token matches the stored
        /// token, and the token has not expired. If any of these conditions are not met, the method returns
        /// null.</remarks>
        /// <param name="userId">The unique identifier of the user whose refresh token is to be validated.</param>
        /// <param name="refreshToken">The refresh token to validate against the user's stored token.</param>
        /// <returns>A user entity if the refresh token is valid and has not expired; otherwise, null.</returns>
        public async Task<UserEntity?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await _context.Users
                .FindAsync(userId);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }
            return user;
        }

        /// <summary>
        /// Generates a cryptographically secure random refresh token encoded as a Base64 string.
        /// </summary>
        /// <remarks>The generated token is suitable for use in authentication scenarios where a unique,
        /// unpredictable value is required, such as issuing refresh tokens for user sessions.</remarks>
        /// <returns>A Base64-encoded string representing a securely generated refresh token.</returns>
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Generates a new refresh token for the specified user and saves it to the data store asynchronously.
        /// </summary>
        /// <remarks>The refresh token is valid for seven days from the time of generation. This method
        /// updates the user's refresh token and its expiry time in the data store.</remarks>
        /// <param name="user">The user entity for which to generate and persist the refresh token. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly generated refresh
        /// token as a string.</returns>
        public async Task<string> GenerateAndSaveRefreshTokenAsync(UserEntity user)
        {
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            
            await _context.SaveChangesAsync();
            
            return refreshToken;
        }
    }

    
}
