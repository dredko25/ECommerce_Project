using ECommerce_Project.Api.DTOs.User;
using ECommerce_Project.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    /// <summary>
    /// Retrieves a list of all registered users.
    /// </summary>
    /// <returns>An HTTP 200 OK response containing a list of user response data transfer objects.</returns>
    [HttpGet]
    public async Task<ActionResult<List<UserResponseDto>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    /// <summary>
    /// Retrieves a specific user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to retrieve.</param>
    /// <returns>An HTTP 200 OK response with the user details if found; 
    /// otherwise, an HTTP 404 Not Found response.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Registers a new user account in the system.
    /// </summary>
    /// <param name="dto">The data transfer object containing the necessary 
    /// information to register the user (e.g., email, password).</param>
    /// <returns>An HTTP 201 Created response containing the newly created user's 
    /// information and tokens if successful; otherwise, an HTTP 409 Conflict 
    /// if the email is already registered.</returns>
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

    /// <summary>
    /// Authenticates a user based on their email and password.
    /// </summary>
    /// <param name="dto">The data transfer object containing the user's login credentials.</param>
    /// <returns>An HTTP 200 OK response with authentication tokens and user details if successful; 
    /// otherwise, an HTTP 401 Unauthorized response.</returns>
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

    /// <summary>
    /// Refreshes the user's authentication tokens using a valid refresh token.
    /// </summary>
    /// <param name="dto">The data transfer object containing the user ID and the current refresh token.</param>
    /// <returns>An HTTP 200 OK response with the new access and refresh tokens if successful; 
    /// otherwise, an HTTP 401 Unauthorized response if the token is invalid or expired.</returns>
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

    /// <summary>
    /// Logs out the currently authenticated user by invalidating 
    /// their active refresh token on the server.
    /// </summary>
    /// <returns>An HTTP 200 OK response if the logout is successful; 
    /// otherwise, an HTTP 400 Bad Request or 401 Unauthorized response.</returns>
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

    /// <summary>
    /// Updates the details of an existing user.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="dto">The data transfer object containing the updated user information.</param>
    /// <returns>An HTTP 200 OK response with the updated user details if successful; 
    /// otherwise, an HTTP 404 Not Found response.</returns>
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> Update(Guid id, UpdateUserDto dto)
    {
        var updated = await _userService.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>
    /// Deletes a user account from the system based on their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>An HTTP 204 No Content response if the deletion is successful; 
    /// otherwise, an HTTP 404 Not Found response if the user does not exist.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _userService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}