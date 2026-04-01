using AutoMapper;
using ECommerce_Project.Api.DTOs.User;
using ECommerce_Project.Api.Interfaces;
using ECommerce_Project.Api.Services;
using ECommerce_Project.DataAccess;
using ECommerce_Project.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Project.Application.Services;

public class UserService : IUserService
{   
    private readonly ECommerceDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly ITokenService _tokenService;

    
    public UserService(ITokenService tokenService, ECommerceDbContext context, IMapper mapper, ILogger<UserService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _tokenService = tokenService;
    }


    /// <summary>
    /// Asynchronously retrieves all users from the data store and maps them to response DTOs.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of user response DTOs. The
    /// list is empty if no users are found.</returns>
    public async Task<List<UserResponseDto>> GetAllAsync()
    {
        var users = await _context.Users
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<UserResponseDto>>(users);
    }

    /// <summary>
    /// Asynchronously retrieves a user by unique identifier and returns the user data as a response DTO.
    /// </summary>
    /// <param name="id">The unique identifier of the user to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="UserResponseDto"/> with 
    /// the user's data if found; otherwise, <see langword="null"/>.</returns>
    public async Task<UserResponseDto?> GetByIdAsync(Guid id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        return user is null ? null : _mapper.Map<UserResponseDto>(user);
    }

    /// <summary>
    /// Creates a new user account and returns an authentication response containing an access token and user information.
    /// </summary>
    /// <param name="dto">The data transfer object containing the information required to create a new user, including email and password.</param>
    /// <returns>An authentication response containing the access token, its expiration time, and the created user's information.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a user with the specified email address already exists.</exception>
    public async Task<AuthResponseDto> CreateAsync(CreateUserDto dto)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == dto.Email);

        if (emailExists)
        {
            _logger.LogWarning("email вже існує: {Email}", dto.Email);
            throw new InvalidOperationException("Email вже зареєстрований");
        }

        var user = _mapper.Map<UserEntity>(dto);
        user.Id = Guid.NewGuid();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        _logger.LogInformation("Зареєстровано! UserId: {UserId}", user.Id);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateAccessToken(user);

        return new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = _mapper.Map<UserResponseDto>(user)
        };
    }

    /// <summary>
    /// Authenticates a user with the provided credentials and generates authentication tokens.
    /// </summary>
    /// <param name="dto">An object containing the user's login credentials, including email and password.</param>
    /// <returns>An <see cref="AuthResponseDto"/> containing the access token, refresh token, expiration time, and user
    /// information if authentication is successful; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the email or password is incorrect.</exception>
    public async Task<AuthResponseDto?> LoginAsync(LoginUserDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Помилка входу: {Email}", dto.Email);
            throw new UnauthorizedAccessException("Неправильний email або пароль");
        }

        _logger.LogInformation("Успішний логін. UserId: {UserId}", user.Id);

        var token = _tokenService.GenerateAccessToken(user);

        var refreshToken = await _tokenService.GenerateAndSaveRefreshTokenAsync(user);

        return new AuthResponseDto
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = _mapper.Map<UserResponseDto>(user)
        };
    }

    /// <summary>
    /// Updates the user with the specified identifier using the provided update data asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="dto">An object containing the updated user information. Only non-null properties will be applied.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a UserResponseDto with the updated
    /// user information, or null if the user is not found.</returns>
    public async Task<UserResponseDto?> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return null;

        if (dto.FirstName != null) user.FirstName = dto.FirstName;
        if (dto.LastName != null) user.LastName = dto.LastName;
        if (dto.ContactNumber != null) user.ContactNumber = dto.ContactNumber;

        await _context.SaveChangesAsync();
        return _mapper.Map<UserResponseDto>(user);
    }

    /// <summary>
    /// Asynchronously deletes the user with the specified unique identifier from the data store.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the user was
    /// found and deleted; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Attempts to refresh the access and refresh tokens for a user based on the provided refresh token request.
    /// </summary>
    /// <remarks>Use this method to obtain new tokens when the current access token has expired. The method
    /// returns null if the provided refresh token is invalid or does not match the user.</remarks>
    /// <param name="dto">The refresh token request containing the user identifier and the refresh token to validate.</param>
    /// <returns>An authentication response containing new access and refresh tokens if the refresh token is valid; otherwise,
    /// null.</returns>
    public async Task<AuthResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto dto)
    {
        var user = await _tokenService.ValidateRefreshTokenAsync(dto.UserId, dto.RefreshToken);
        if (user is null)
        {
            _logger.LogWarning("Невалідний refresh token для UserId: {UserId}", dto.UserId);
            return null;
        }
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = await _tokenService.GenerateAndSaveRefreshTokenAsync(user);

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = _mapper.Map<UserResponseDto>(user)
        };
    }

    /// <summary>
    /// Logs out the specified user by invalidating their refresh token asynchronously.
    /// </summary>
    /// <remarks>This method removes the user's refresh token, effectively ending their session. If the user
    /// does not exist, no action is taken and the method returns false.</remarks>
    /// <param name="userId">The unique identifier of the user to log out.</param>
    /// <returns>true if the user was found and logged out successfully; otherwise, false.</returns>
    public async Task<bool> LogoutAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user is null)
        {
            _logger.LogWarning("Спроба logout для неіснуючого користувача з ID: {UserId}", userId);
            return false;
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Користувач успішно вийшов. Токен видалено. UserId: {UserId}", userId);

        return true;
    }
}