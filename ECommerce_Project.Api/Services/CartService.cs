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

    /// <summary>
    /// Retrieves a queryable collection of carts, including their associated cart items and products.
    /// </summary>
    /// <remarks>The returned query enables further composition with additional LINQ operations before
    /// execution. The related cart items and products are eagerly loaded to avoid additional database queries when
    /// accessing these navigation properties.</remarks>
    /// <returns>An <see cref="IQueryable{CartEntity}"/> that includes each cart's items and the corresponding products.</returns>
    private IQueryable<CartEntity> GetCartWithItems() =>
        _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(i => i.Product);

    /// <summary>
    /// Retrieves the shopping cart and its items for the specified user asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose cart is to be retrieved.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="CartResponseDto"/>
    /// representing the user's cart and its items, or <see langword="null"/> if no cart exists for the specified user.</returns>
    public async Task<CartResponseDto?> GetByUserAsync(Guid userId)
    {
        var cart = await GetCartWithItems()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId);

        return _mapper.Map<CartResponseDto?>(cart);
    }

    /// <summary>
    /// Adds an item to the user's shopping cart asynchronously, creating a new cart if one does not exist.
    /// </summary>
    /// <remarks>If the specified product already exists in the cart, its quantity is increased by the
    /// specified amount. Otherwise, a new item is added to the cart. If the user does not have an existing cart, a new
    /// cart is created.</remarks>
    /// <param name="userId">The unique identifier of the user whose cart will be updated.</param>
    /// <param name="dto">An object containing the product identifier and quantity to add to the cart. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a data transfer object representing
    /// the updated state of the user's cart.</returns>
    public async Task<CartResponseDto> AddItemAsync(Guid userId, AddToCartDto dto)
    {
        var product = await _context.Products
            .FindAsync(dto.ProductId);
        if (product == null) 
            throw new Exception("Товар не знайдено");

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
        int requestedQuantity = (existingItem?.Quantity ?? 0) + dto.Quantity;

        if (requestedQuantity > product.QuantityAvailable)
            throw new InvalidOperationException($"На складі доступно лише {product.QuantityAvailable} шт.");

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

    /// <summary>
    /// Updates the quantity of a specific item in the user's shopping cart.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose cart is being updated.</param>
    /// <param name="cartItemId">The unique identifier of the cart item to update.</param>
    /// <param name="quantity">The new quantity for the cart item. If less than or equal to zero, the item is removed from the cart.</param>
    /// <returns>A <see cref="CartResponseDto"/> representing the updated cart, or <see langword="null"/> if the cart or item
    /// does not exist.</returns>
    public async Task<CartResponseDto?> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, int quantity)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(i => i.Product)
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
            if (quantity > item.Product?.QuantityAvailable)
            {
                throw new InvalidOperationException($"Максимально доступна кількість: {item.Product.QuantityAvailable} шт.");
            }
            item.Quantity = quantity;
        }

        await _context.SaveChangesAsync();

        var updatedCart = await GetCartWithItems()
            .AsNoTracking()
            .FirstAsync(c => c.Id == cart.Id);

        return _mapper.Map<CartResponseDto>(updatedCart);
    }

    /// <summary>
    /// Removes a specific item from the user's shopping cart asynchronously and returns the updated cart information.
    /// </summary>
    /// <remarks>Returns <see langword="null"/> if the specified cart or cart item does not exist for the
    /// given user.</remarks>
    /// <param name="userId">The unique identifier of the user whose cart is being modified.</param>
    /// <param name="cartItemId">The unique identifier of the cart item to remove from the user's cart.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="CartResponseDto"/> with
    /// the updated cart information if the cart and item exist; otherwise, <see langword="null"/>.</returns>
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

    /// <summary>
    /// Asynchronously removes all items from the shopping cart associated with the specified user.
    /// </summary>
    /// <remarks>If the user does not have an existing cart, the method completes without performing any
    /// action.</remarks>
    /// <param name="userId">The unique identifier of the user whose cart will be cleared.</param>
    /// <returns>A task that represents the asynchronous clear operation.</returns>
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