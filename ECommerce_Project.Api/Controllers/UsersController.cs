using ECommerce_Project.Api.DTOs.User;
using ECommerce_Project.Api.Interfaces;
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

            _logger.LogInformation("POST /login — успіх, UserId: {UserId}", result.User.Id);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("POST /login — невдача для email: {Email}", dto.Email);
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> Update(Guid id, UpdateUserDto dto)
    {
        var updated = await _userService.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _userService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}