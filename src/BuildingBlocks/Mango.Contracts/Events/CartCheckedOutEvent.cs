namespace Mango.Contracts.Events;

/// <summary>
/// Published when a cart is checked out (user proceeds to order)
/// </summary>
public record CartCheckedOutEvent(
    string UserId,
    Guid CartId,
    List<CartItemEvent> Items,
    decimal TotalAmount,
    string? CouponCode,
    int? RewardPointsUsed,
    DateTime CheckedOutAt
);

public record CartItemEvent(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    decimal Total
);
