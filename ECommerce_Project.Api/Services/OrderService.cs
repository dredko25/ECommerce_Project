using AutoMapper;
using ECommerce_Project.Api.DTOs.Order;
using ECommerce_Project.Api.Interfaces;
using ECommerce_Project.DataAccess;
using ECommerce_Project.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

public class OrderService : IOrderService
{
    private readonly ECommerceDbContext _context;
    private readonly IMapper _mapper;

    public OrderService(ECommerceDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<OrderSummaryDto>> GetAllAsync()
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return _mapper.Map<List<OrderSummaryDto>>(orders);
    }

    public async Task<List<OrderSummaryDto>> GetByUserAsync(Guid userId)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return _mapper.Map<List<OrderSummaryDto>>(orders);
    }

    public async Task<OrderResponseDto?> GetByIdAsync(Guid id)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        return _mapper.Map<OrderResponseDto?>(order);
    }

    public async Task<OrderResponseDto> CreateAsync(Guid userId, CreateOrderDto dto)
    {
        var order = _mapper.Map<OrderEntity>(dto);

        order.Id = Guid.NewGuid();
        order.UserId = userId;
        order.OrderDate = DateTime.UtcNow;
        order.Status = OrderStatus.Pending;
        order.OrderNumber =
            $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        var productIds = dto.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        order.OrderItems = dto.Items.Select(item =>
        {
            var product = products[item.ProductId];
            return new OrderItemEntity
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = product.Price
            };
        }).ToList();

        order.TotalAmount = order.OrderItems.Sum(i => i.Price * i.Quantity);

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(order.Id)
            ?? throw new Exception("Order not found after create");
    }

    public async Task<OrderResponseDto?> UpdateStatusAsync(Guid id, string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null) return null;

        if (!Enum.TryParse<OrderStatus>(status, out var parsed))
            throw new ArgumentException($"Invalid status: {status}");

        order.Status = parsed;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null) return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }
}