using Mango.Services.CouponAPI.Domain;
using Mango.Services.CouponAPI.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.CouponAPI.Services;

public interface ICouponService
{
    Task<IEnumerable<CouponDto>> GetCouponsAsync(CancellationToken cancellationToken = default);
    Task<CouponDto?> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<CouponDto> CreateCouponAsync(CreateCouponDto dto, CancellationToken cancellationToken = default);
}

public class CouponService : ICouponService
{
    private readonly CouponDbContext _context;
    private readonly ILogger<CouponService> _logger;

    public CouponService(CouponDbContext context, ILogger<CouponService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<CouponDto>> GetCouponsAsync(CancellationToken cancellationToken = default)
    {
        var coupons = await _context.Coupons.Where(c => c.IsActive).ToListAsync(cancellationToken);
        return coupons.Select(MapToDto);
    }

    public async Task<CouponDto?> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code && c.IsActive, cancellationToken);
        if (coupon == null) return null;

        if (coupon.ExpirationDate < DateTime.UtcNow || coupon.CurrentUsages >= coupon.MaxUsages)
            return null;

        return MapToDto(coupon);
    }

    public async Task<CouponDto> CreateCouponAsync(CreateCouponDto dto, CancellationToken cancellationToken = default)
    {
        var coupon = new Coupon
        {
            Code = dto.Code,
            Description = dto.Description,
            DiscountAmount = dto.DiscountAmount,
            MinAmount = dto.MinAmount,
            ExpirationDate = dto.ExpirationDate,
            MaxUsages = dto.MaxUsages,
            IsActive = true
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Coupon {Code} created", coupon.Code);
        return MapToDto(coupon);
    }

    private static CouponDto MapToDto(Coupon c) => new(c.Id, c.Code, c.Description, c.DiscountAmount, c.MinAmount, c.ExpirationDate, c.IsActive, c.MaxUsages, c.CurrentUsages);
}
