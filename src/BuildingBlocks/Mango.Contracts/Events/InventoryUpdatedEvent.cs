namespace Mango.Contracts.Events;

/// <summary>
/// Published when product inventory is updated
/// </summary>
public record InventoryUpdatedEvent(
    int ProductId,
    string ProductName,
    int QuantityChanged,
    int NewQuantity,
    bool IsRestock,
    DateTime UpdatedAt
);
