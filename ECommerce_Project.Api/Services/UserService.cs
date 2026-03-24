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

    public async Task<List<UserResponseDto>> GetAllAsync()
    {
        var users = await _context.Users
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<UserResponseDto>>(users);
    }

    public async Task<UserResponseDto?> GetByIdAsync(Guid id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        return user is null ? null : _mapper.Map<UserResponseDto>(user);
    }

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

    public async Task<AuthResponseDto> LoginAsync(LoginUserDto dto)
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

        return new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = _mapper.Map<UserResponseDto>(user)
        };
    }

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

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}