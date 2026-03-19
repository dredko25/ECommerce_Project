using AutoMapper;
using ECommerce_Project.Api.DTOs.User;
using ECommerce_Project.Api.Interfaces;
using ECommerce_Project.DataAccess;
using ECommerce_Project.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Project.Application.Services;

public class UserService : IUserService
{
    private readonly ECommerceDbContext _context;
    private readonly IMapper _mapper;

    public UserService(ECommerceDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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

    public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
    {
        var user = _mapper.Map<UserEntity>(dto);
        user.Id = Guid.NewGuid();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return _mapper.Map<UserResponseDto>(user);
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