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

    private IQueryable<CartEntity> CartWithItems =>
        _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(i => i.Product);

    public async Task<CartResponseDto?> GetByUserAsync(Guid userId)
    {
        var cart = await CartWithItems
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId);

        return cart is null ? null : _mapper.Map<CartResponseDto>(cart);
    }

    public async Task<CartResponseDto> AddItemAsync(Guid userId, AddToCartDto dto)
    {
        var cart = await CartWithItems
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null)
        {
            cart = new CartEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                CartItems = []
            };
            await _context.Carts.AddAsync(cart);
        }

        var existing = cart.CartItems.FirstOrDefault(i => i.ProductId == dto.ProductId);
        if (existing is not null)
        {
            existing.Quantity += dto.Quantity;
        }
        else
        {
            cart.CartItems.Add(new CartItemEntity
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            });
        }

        await _context.SaveChangesAsync();

        var updated = await CartWithItems
            .AsNoTracking()
            .FirstAsync(c => c.Id == cart.Id);

        return _mapper.Map<CartResponseDto>(updated);
    }

    public async Task<CartResponseDto?> UpdateItemQuantityAsync(
        Guid userId, Guid cartItemId, int quantity)
    {
        var cart = await CartWithItems
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart is null) return null;

        var item = cart.CartItems.FirstOrDefault(i => i.Id == cartItemId);
        if (item is null) return null;

        if (quantity <= 0)
            cart.CartItems.Remove(item);
        else
            item.Quantity = quantity;

        await _context.SaveChangesAsync();

        var updated = await CartWithItems
            .AsNoTracking()
            .FirstAsync(c => c.Id == cart.Id);

        return _mapper.Map<CartResponseDto>(updated);
    }

    public async Task<CartResponseDto?> RemoveItemAsync(Guid userId, Guid cartItemId)
    {
        var cart = await CartWithItems
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart is null) return null;

        var item = cart.CartItems.FirstOrDefault(i => i.Id == cartItemId);
        if (item is not null)
        {
            cart.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        var updated = await CartWithItems
            .AsNoTracking()
            .FirstAsync(c => c.Id == cart.Id);

        return _mapper.Map<CartResponseDto>(updated);
    }

    public async Task ClearAsync(Guid userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null) return;

        cart.CartItems.Clear();
        await _context.SaveChangesAsync();
    }
}