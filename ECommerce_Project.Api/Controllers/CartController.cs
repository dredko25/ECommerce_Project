using ECommerce_Project.Api.DTOs.Cart;
using ECommerce_Project.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<CartResponseDto>> AddItem(
        Guid userId, AddToCartDto dto)
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

    [HttpDelete("user/{userId:guid}/items/{cartItemId:guid}")]
    public async Task<ActionResult<CartResponseDto>> RemoveItem(
        Guid userId, Guid cartItemId)
    {
        var cart = await _cartService.RemoveItemAsync(userId, cartItemId);
        return cart is null ? NotFound() : Ok(cart);
    }

    [HttpDelete("user/{userId:guid}")]
    public async Task<IActionResult> Clear(Guid userId)
    {
        await _cartService.ClearAsync(userId);
        return NoContent();
    }
}