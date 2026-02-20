namespace Mango.Contracts.Events;

/// <summary>
/// Published when reward points are redeemed for a discount
/// </summary>
public record PointsRedeemedEvent(
    string UserId,
    int PointsRedeemed,
    decimal DiscountAmount,
    Guid? OrderId,
    DateTime RedeemedAt
);
