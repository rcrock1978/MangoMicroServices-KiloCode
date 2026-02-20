using Mango.Contracts.Responses;
using Mango.Services.AuthAPI.DTO;
using Mango.Services.AuthAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AuthAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(
        [FromBody] LoginRequestDto request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            return Ok(ApiResponse<LoginResponseDto>.Success(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Login failed for user {UserName}", request.UserName);
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {UserName}", request.UserName);
            return StatusCode(500, ApiResponse<LoginResponseDto>.Fail("An error occurred during login"));
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<string>>> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = await _authService.RegisterAsync(request, cancellationToken);
            return Ok(ApiResponse<string>.Success(userId, statusCode: 201, message: "User registered successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Registration failed for email {Email}", request.Email);
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email {Email}", request.Email);
            return StatusCode(500, ApiResponse<string>.Fail("An error occurred during registration"));
        }
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetUsers(CancellationToken cancellationToken)
    {
        try
        {
            var users = await _authService.GetUsersAsync(cancellationToken);
            return Ok(ApiResponse<IEnumerable<UserDto>>.Success(users));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, ApiResponse<IEnumerable<UserDto>>.Fail("An error occurred while retrieving users"));
        }
    }

    [HttpGet("users/{userId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(
        string userId, 
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _authService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.Fail("User not found"));
            }
            return Ok(ApiResponse<UserDto>.Success(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", userId);
            return StatusCode(500, ApiResponse<UserDto>.Fail("An error occurred while retrieving user"));
        }
    }
}
