namespace Mango.Services.AuthAPI.DTO;

public record LoginRequestDto(
    string UserName,
    string Password
);

public record RegisterRequestDto(
    string Email,
    string Name,
    string Password,
    string? PhoneNumber,
    string? StreetAddress,
    string? City,
    string? State,
    string? PostalCode
);

public record UserDto(
    string Id,
    string Email,
    string Name,
    string? PhoneNumber,
    string? StreetAddress,
    string? City,
    string? State,
    string? PostalCode
);

public record LoginResponseDto(
    string UserId,
    string Email,
    string Name,
    string Token
);

public record JwtOptions(
    string Secret,
    string Issuer,
    string Audience,
    int ExpirationMinutes
);
