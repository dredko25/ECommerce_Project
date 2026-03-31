using ECommerce_Project.Api.DTOs.Order;
using ECommerce_Project.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderSummaryDto>>> GetAll()
    {
        var orders = await _orderService.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<List<OrderSummaryDto>>> GetByUser(Guid userId)
    {
        var orders = await _orderService.GetByUserAsync(userId);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponseDto>> GetById(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("user/{userId:guid}")]
    public async Task<ActionResult<OrderResponseDto>> Create(Guid userId, CreateOrderDto dto)
    {
        try
        {
            var created = await _orderService.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Внутрішня помилка сервера при створенні замовлення." });
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<OrderResponseDto>> UpdateStatus(
        Guid id, [FromBody] string status)
    {
        var updated = await _orderService.UpdateStatusAsync(id, status);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _orderService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}