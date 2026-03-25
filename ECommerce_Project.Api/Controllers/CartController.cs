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

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<CartResponseDto>> GetByUser(Guid userId)
    {
        var cart = await _cartService.GetByUserAsync(userId);
        return cart is null ? NotFound() : Ok(cart);
    }
    
    [HttpPost("user/{userId:guid}/items")]
    public async Task<ActionResult<CartResponseDto>> AddItem(Guid userId, [FromBody] AddToCartDto dto)
    {
        var cart = await _cartService.AddItemAsync(userId, dto);
        return Ok(cart);
    }

    [HttpPatch("user/{userId:guid}/items/{cartItemId:guid}")]
    public async Task<ActionResult<CartResponseDto>> UpdateQuantity(
        Guid userId, Guid cartItemId, [FromBody] int quantity)
    {
        var cart = await _cartService.UpdateItemQuantityAsync(userId, cartItemId, quantity);
        return cart is null ? NotFound() : Ok(cart);
    }

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

    [HttpDelete("user/{userId:guid}")]
    public async Task<IActionResult> Clear(Guid userId)
    {
        await _cartService.ClearAsync(userId);
        return NoContent();
    }
}