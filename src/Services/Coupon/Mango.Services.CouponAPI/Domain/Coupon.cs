using Mango.SharedKernel;

namespace Mango.Services.CouponAPI.Domain;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal MinAmount { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int MaxUsages { get; set; }
    public int CurrentUsages { get; set; }
}
