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

    /// <summary>
    /// Retrieves a list of all orders in the system.
    /// </summary>
    /// <returns>An HTTP 200 OK response containing a list of order summaries.</returns>
    [HttpGet]
    public async Task<ActionResult<List<OrderSummaryDto>>> GetAll()
    {
        var orders = await _orderService.GetAllAsync();
        return Ok(orders);
    }

    /// <summary>
    /// Retrieves a list of orders placed by a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user 
    /// whose orders are to be retrieved.</param>
    /// <returns>An HTTP 200 OK response containing a list of 
    /// the user's order summaries.</returns>
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<List<OrderSummaryDto>>> GetByUser(Guid userId)
    {
        var orders = await _orderService.GetByUserAsync(userId);
        return Ok(orders);
    }

    /// <summary>
    /// Retrieves the detailed information of a specific order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order to retrieve.</param>
    /// <returns>An HTTP 200 OK response with the order details if found; 
    /// otherwise, an HTTP 404 Not Found response.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponseDto>> GetById(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>
    /// Creates a new order for a specific user based on the provided order details.
    /// </summary>
    /// <param name="userId">The unique identifier of the user placing the order.</param>
    /// <param name="dto">The data transfer object containing the order information.</param>
    /// <returns>An HTTP 201 Created response with the newly created order details if successful; 
    /// an HTTP 400 Bad Request if validation fails; 
    /// or an HTTP 500 Internal Server Error if an unexpected issue occurs.</returns>
    [HttpPost("user/{userId:guid}")]
    public async Task<ActionResult<OrderResponseDto>> Create(
        Guid userId, CreateOrderDto dto)
    {
        var created = await _orderService.CreateAsync(userId, dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates the status of an existing order.
    /// </summary>
    /// <param name="id">The unique identifier of the order to update.</param>
    /// <param name="status">The new status to be applied to the order.</param>
    /// <returns>An HTTP 200 OK response with the updated order details if successful; 
    /// otherwise, an HTTP 404 Not Found response.</returns>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<OrderResponseDto>> UpdateStatus(
        Guid id, [FromBody] string status)
    {
        var updated = await _orderService.UpdateStatusAsync(id, status);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>
    /// Deletes a specific order from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the order to delete.</param>
    /// <returns>An HTTP 204 No Content response if the deletion is successful; 
    /// otherwise, an HTTP 404 Not Found response.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _orderService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}