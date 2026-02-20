namespace Mango.Contracts.Events;

/// <summary>
/// Published when payment for an order is completed
/// </summary>
public record PaymentCompletedEvent(
    Guid OrderId,
    string PaymentId,
    string UserId,
    decimal Amount,
    bool IsSuccessful,
    DateTime CompletedAt
);
