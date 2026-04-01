using ECommerce_Project.Api.DTOs.Cart;
using ECommerce_Project.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Retrieves the active shopping cart and its items for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose cart is to be retrieved.</param>
    /// <returns>An HTTP 200 OK response with the cart details if found; 
    /// otherwise, an HTTP 404 Not Found response.</returns>
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<CartResponseDto>> GetByUser(Guid userId)
    {
        var cart = await _cartService.GetByUserAsync(userId);
        return cart is null ? NotFound() : Ok(cart);
    }

    /// <summary>
    /// Adds a new item to the user's shopping cart or increments the quantity if the item already exists.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="dto">The data transfer object containing the product ID and quantity to add.</param>
    /// <returns>An HTTP 200 OK response with the updated cart details if successful; 
    /// or an HTTP 400 Bad Request if validation fails (e.g., requested quantity exceeds available stock).</returns>
    [HttpPost("user/{userId:guid}/items")]
    public async Task<ActionResult<CartResponseDto>> AddItem(Guid userId, [FromBody] AddToCartDto dto)
    {
        try
        {
            var cart = await _cartService.AddItemAsync(userId, dto);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates the quantity of a specific item currently in the user's shopping cart.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cartItemId">The unique identifier of the cart item to update.</param>
    /// <param name="quantity">The new required quantity for the cart item.</param>
    /// <returns>An HTTP 200 OK response with the updated cart details if successful; 
    /// an HTTP 400 Bad Request if validation fails (e.g., exceeding available stock); 
    /// or an HTTP 404 Not Found if the cart or item does not exist.</returns>
    [HttpPatch("user/{userId:guid}/items/{cartItemId:guid}")]
    public async Task<ActionResult<CartResponseDto>> UpdateQuantity(Guid userId, Guid cartItemId, [FromBody] int quantity)
    {
        try
        {
            var cart = await _cartService.UpdateItemQuantityAsync(userId, cartItemId, quantity);
            return cart is null ? NotFound() : Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Removes a specific item from the authenticated user's shopping cart.
    /// </summary>
    /// <param name="cartItemId">The unique identifier of the cart item to remove.</param>
    /// <returns>An HTTP 200 OK response with the updated cart details if the item was successfully removed; 
    /// an HTTP 401 Unauthorized if the user is not authenticated properly; 
    /// or an HTTP 404 Not Found if the item is not found in the cart.</returns>
    [HttpDelete("items/{cartItemId:guid}")]
    public async Task<ActionResult<CartResponseDto>> RemoveItem(Guid cartItemId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            return Unauthorized("Користувач не авторизований");

        var result = await _cartService.RemoveItemAsync(userId, cartItemId);

        if (result == null)
            return NotFound("Товар не знайдено в кошику");

        return Ok(result);
    }

    /// <summary>
    /// Clears all items from a specific user's shopping cart.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose cart will be cleared.</param>
    /// <returns>An HTTP 204 No Content response indicating the cart was successfully cleared.</returns>
    [HttpDelete("user/{userId:guid}")]
    public async Task<IActionResult> Clear(Guid userId)
    {
        await _cartService.ClearAsync(userId);
        return NoContent();
    }
}