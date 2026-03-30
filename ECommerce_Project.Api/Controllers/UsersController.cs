using ECommerce_Project.Api.DTOs.User;
using ECommerce_Project.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce_Project.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponseDto>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponseDto>> Create(CreateUserDto dto)
    {
        _logger.LogInformation("POST /register — спроба для email: {Email}", dto.Email);

        try
        {
            var created = await _userService.CreateAsync(dto);

            _logger.LogInformation("POST /register — успіх, UserId: {UserId}", created.User.Id);

            return CreatedAtAction(
                actionName: nameof(GetById),
                controllerName: "Users",
                routeValues: new { id = created.User.Id },
                value: created);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("POST /register — конфлікт: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserResponseDto>> Login(LoginUserDto dto)
    {
        _logger.LogInformation("POST /login — спроба для email: {Email}", dto.Email);

        try
        {
            var result = await _userService.LoginAsync(dto);

            _logger.LogInformation("POST /login — успіх, UserId: {UserId}", result?.User.Id);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("POST /login — невдача для email: {Email}", dto.Email);
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<UserResponseDto>> RefreshToken(RefreshTokenRequestDto dto)
    {
        _logger.LogInformation("POST /refresh-token — спроба для UserId: {UserId}", dto.UserId);
        try
        {
            var result = await _userService.RefreshTokensAsync(dto);

            if (result == null)
            {
                _logger.LogWarning("POST /refresh-token — відмовлено для UserId: {UserId} (недійсний токен)", dto.UserId);
                return Unauthorized(new { message = "Недійсний refresh token. Увійдіть знову." });
            }

            _logger.LogInformation("POST /refresh-token — успіх для UserId: {UserId}", dto.UserId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("POST /refresh-token — невдача для UserId: {UserId}", dto.UserId);
            return Unauthorized(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(new { message = "Недійсний токен" });
        }

        var success = await _userService.LogoutAsync(userId);

        if (!success)
        {
            return BadRequest(new { message = "Не вдалося виконати вихід" });
        }

        return Ok(new { message = "Ви успішно вийшли з системи" });
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> Update(Guid id, UpdateUserDto dto)
    {
        var updated = await _userService.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    //[Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _userService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}