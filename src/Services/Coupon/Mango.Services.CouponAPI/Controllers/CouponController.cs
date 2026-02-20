using Mango.Contracts.Responses;
using Mango.Services.CouponAPI.DTO;
using Mango.Services.CouponAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.CouponAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponController : ControllerBase
{
    private readonly ICouponService _couponService;
    private readonly ILogger<CouponController> _logger;

    public CouponController(ICouponService couponService, ILogger<CouponController> logger)
    {
        _couponService = couponService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CouponDto>>>> GetCoupons(CancellationToken cancellationToken)
    {
        var coupons = await _couponService.GetCouponsAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<CouponDto>>.Success(coupons));
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<ApiResponse<CouponDto>>> GetCoupon(string code, CancellationToken cancellationToken)
    {
        var coupon = await _couponService.GetCouponByCodeAsync(code, cancellationToken);
        if (coupon == null)
            return NotFound(ApiResponse<CouponDto>.Fail("Coupon not found or expired"));
        return Ok(ApiResponse<CouponDto>.Success(coupon));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CouponDto>>> CreateCoupon([FromBody] CreateCouponDto dto, CancellationToken cancellationToken)
    {
        var coupon = await _couponService.CreateCouponAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetCoupon), new { code = coupon.Code }, ApiResponse<CouponDto>.Success(coupon, 201));
    }
}
