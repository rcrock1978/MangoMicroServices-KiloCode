namespace Mango.Contracts.Events;

/// <summary>
/// Published when reward points are earned from a purchase
/// </summary>
public record RewardPointsEarnedEvent(
    string UserId,
    int PointsEarned,
    decimal PurchaseAmount,
    Guid? OrderId,
    DateTime EarnedAt
);
