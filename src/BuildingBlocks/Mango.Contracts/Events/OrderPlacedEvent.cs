namespace Mango.Contracts.Events;

/// <summary>
/// Published when an order is placed
/// </summary>
public record OrderPlacedEvent(
    Guid OrderId,
    string UserId,
    string UserEmail,
    string UserName,
    decimal TotalAmount,
    DateTime CreatedAt,
    List<OrderItemEvent> Items
);

public record OrderItemEvent(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    decimal Total
);
