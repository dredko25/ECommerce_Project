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
    private readonly ILogger<OrderService> _logger;

    public OrderService(ECommerceDbContext context, IMapper mapper, ILogger<OrderService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously retrieves all orders, including their associated order items, and returns them as a list of order
    /// summary data transfer objects.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
    /// cref="OrderSummaryDto"/> objects representing all orders, ordered by descending order date. The list is empty if
    /// no orders are found.</returns>
    public async Task<List<OrderSummaryDto>> GetAllAsync()
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return _mapper.Map<List<OrderSummaryDto>>(orders);
    }

    /// <summary>
    /// Asynchronously retrieves a list of order summaries for the specified user, ordered by most recent order date.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose orders are to be retrieved.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of order summary data
    /// transfer objects for the specified user. The list is empty if the user has no orders.</returns>
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

    /// <summary>
    /// Asynchronously retrieves an order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="OrderResponseDto"/>
    /// representing the order if found; otherwise, <see langword="null"/>.</returns>
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

    /// <summary>
    /// Creates a new order for the specified user using the provided order details.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom the order is being created.</param>
    /// <param name="dto">An object containing the details of the order to create, including items and quantities. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an OrderResponseDto with the details
    /// of the newly created order.</returns>
    /// <exception cref="Exception">Thrown if the order cannot be retrieved after creation.</exception>
    public async Task<OrderResponseDto> CreateAsync(Guid userId, CreateOrderDto dto)
    {
        _logger.LogInformation("Розпочато оформлення замовлення для користувача {UserId}.", userId);

        var order = _mapper.Map<OrderEntity>(dto);

        order.Id = Guid.NewGuid();
        order.UserId = userId;
        order.OrderDate = DateTime.UtcNow;
        order.Status = OrderStatus.Pending;
        order.OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..5].ToUpper()}";

        var productIds = dto.Items.Select(i => i.ProductId).ToList();

        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        order.OrderItems = new List<OrderItemEntity>();

        foreach (var item in dto.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
            {
                _logger.LogWarning("При оформленні замовлення користувачем {UserId} не знайдено товар {ProductId}.", userId, item.ProductId);
                throw new Exception($"Товар з ID {item.ProductId} не знайдено.");
            }

            if (item.Quantity > product.QuantityAvailable)
            {
                _logger.LogWarning("Недостатньо товару {ProductId} для замовлення користувача {UserId}. Замовлено: {RequestedQty}, доступно: {AvailableQty}.",
                    product.Id, userId, item.Quantity, product.QuantityAvailable);

                throw new InvalidOperationException(
                    $"Недостатньо товару '{product.Name}' на складі. Доступно: {product.QuantityAvailable}, ви замовляєте: {item.Quantity}.");
            }

            product.QuantityAvailable -= item.Quantity;

            order.OrderItems.Add(new OrderItemEntity
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = product.Price
            });
        }

        order.TotalAmount = order.OrderItems.Sum(i => i.Price * i.Quantity);

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var createdOrder = await GetByIdAsync(order.Id);

        if (createdOrder == null)
        {
            _logger.LogError("Замовлення {OrderId} не знайдено в БД після збереження.", order.Id);
            throw new Exception("Замовлення не знайдено після створення.");
        }

        _logger.LogInformation("Замовлення {OrderId} (Номер: {OrderNumber}) на суму {TotalAmount} успішно створено для користувача {UserId}.",
            order.Id, order.OrderNumber, order.TotalAmount, userId);

        return createdOrder;
    }

    /// <summary>
    /// Updates the status of the specified order asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the order to update.</param>
    /// <param name="status">The new status to assign to the order. Must be a valid value of the OrderStatus enumeration.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an OrderResponseDto representing the
    /// updated order, or null if the order does not exist.</returns>
    /// <exception cref="ArgumentException">Thrown if the status parameter does not correspond to a valid OrderStatus value.</exception>
    public async Task<OrderResponseDto?> UpdateStatusAsync(Guid id, string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null)
        {
            _logger.LogWarning("Спроба оновити статус неіснуючого замовлення з ID {OrderId}.", id);
            return null;
        }

        if (!Enum.TryParse<OrderStatus>(status, out var parsed))
        {
            _logger.LogWarning("Спроба встановити неіснуючий статус '{Status}' для замовлення {OrderId}.", status, id);
            throw new ArgumentException($"Invalid status: {status}");
        }

        var oldStatus = order.Status;
        order.Status = parsed;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Статус замовлення {OrderId} змінено з {OldStatus} на {NewStatus}.", id, oldStatus, parsed);

        return await GetByIdAsync(id);
    }

    /// <summary>
    /// Asynchronously deletes the order with the specified identifier, if it exists.
    /// </summary>
    /// <param name="id">The unique identifier of the order to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the order was
    /// found and deleted; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null)
        {
            _logger.LogWarning("Спроба видалити неіснуюче замовлення з ID {OrderId}.", id);
            return false;
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Замовлення {OrderId} успішно видалено.", id);
        return true;
    }
}