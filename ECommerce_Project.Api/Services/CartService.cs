using AutoMapper;
using ECommerce_Project.Api.DTOs.Cart;
using ECommerce_Project.Api.Interfaces;
using ECommerce_Project.DataAccess;
using ECommerce_Project.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

public class CartService : ICartService
{
    private readonly ECommerceDbContext _context;
    private readonly IMapper _mapper;

    public CartService(ECommerceDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    private IQueryable<CartEntity> GetCartWithItems() =>
        _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(i => i.Product);

    public async Task<CartResponseDto?> GetByUserAsync(Guid userId)
    {
        var cart = await GetCartWithItems()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId);

        return _mapper.Map<CartResponseDto?>(cart);
    }

    public async Task<CartResponseDto> AddItemAsync(Guid userId, AddToCartDto dto)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new CartEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        var existingItem = cart.CartItems.FirstOrDefault(i => i.ProductId == dto.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += dto.Quantity;
        }
        else
        {
            var newItem = new CartItemEntity
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };

            _context.CartItems.Add(newItem);
        }

        await _context.SaveChangesAsync();

        var updatedCart = await GetCartWithItems()
            .AsNoTracking()
            .FirstAsync(c => c.Id == cart.Id);

        return _mapper.Map<CartResponseDto>(updatedCart);
    }

    public async Task<CartResponseDto?> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, int quantity)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null) return null;

        var item = cart.CartItems.FirstOrDefault(i => i.Id == cartItemId);
        if (item == null) return null;

        if (quantity <= 0)
        {
            _context.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        await _context.SaveChangesAsync();

        var updatedCart = await GetCartWithItems()
            .AsNoTracking()
            .FirstAsync(c => c.Id == cart.Id);

        return _mapper.Map<CartResponseDto>(updatedCart);
    }

    public async Task<CartResponseDto?> RemoveItemAsync(Guid userId, Guid cartItemId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null) return null;

        var item = cart.CartItems.FirstOrDefault(i => i.Id == cartItemId);
        if (item == null) return null;

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();

        var updatedCart = await GetCartWithItems()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cart.Id);

        return _mapper.Map<CartResponseDto>(updatedCart);
    }

    public async Task ClearAsync(Guid userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null) return;

        _context.CartItems.RemoveRange(cart.CartItems);
        await _context.SaveChangesAsync();
    }
}