namespace Mango.Contracts.Events;

/// <summary>
/// Published when a new user registers
/// </summary>
public record UserRegisteredEvent(
    string UserId,
    string Email,
    string Name,
    DateTime RegisteredAt
);
