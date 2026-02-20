using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Mango.Contracts.Events;
using Mango.MessageBus;
using Mango.Services.AuthAPI.DTO;
using Mango.Services.AuthAPI.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Mango.Services.AuthAPI.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<string> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
}

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtOptions _jwtOptions;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JwtOptions> jwtOptions,
        IMessageBus messageBus,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtOptions = jwtOptions.Value;
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        var token = await GenerateJwtTokenAsync(user);

        _logger.LogInformation("User {UserName} logged in successfully", request.UserName);

        return new LoginResponseDto(user.Id, user.Email!, user.Name, token);
    }

    public async Task<string> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            StreetAddress = request.StreetAddress,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        // Publish UserRegistered event for EmailAPI to consume
        var userRegisteredEvent = new UserRegisteredEvent(
            user.Id,
            user.Email!,
            user.Name,
            DateTime.UtcNow
        );
        
        await _messageBus.PublishAsync(userRegisteredEvent, cancellationToken);

        _logger.LogInformation("User {Email} registered successfully", request.Email);

        return user.Id;
    }

    public async Task<IEnumerable<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken);
        return users.Select(u => new UserDto(u.Id, u.Email!, u.Name, u.PhoneNumber, 
            u.StreetAddress, u.City, u.State, u.PostalCode));
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;
        
        return new UserDto(user.Id, user.Email!, user.Name, user.PhoneNumber,
            user.StreetAddress, user.City, user.State, user.PostalCode);
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
