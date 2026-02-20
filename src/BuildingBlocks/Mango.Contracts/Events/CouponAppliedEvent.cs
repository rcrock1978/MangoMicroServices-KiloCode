namespace Mango.Contracts.Events;

/// <summary>
/// Published when a coupon is applied to a cart or order
/// </summary>
public record CouponAppliedEvent(
    string CouponCode,
    string UserId,
    decimal DiscountAmount,
    decimal OriginalAmount,
    decimal FinalAmount,
    DateTime AppliedAt
);
