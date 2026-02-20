namespace Mango.Services.CouponAPI.DTO;

public record CouponDto(int Id, string Code, string Description, decimal DiscountAmount, decimal MinAmount, DateTime ExpirationDate, bool IsActive, int MaxUsages, int CurrentUsages);
public record CreateCouponDto(string Code, string Description, decimal DiscountAmount, decimal MinAmount, DateTime ExpirationDate, int MaxUsages);
