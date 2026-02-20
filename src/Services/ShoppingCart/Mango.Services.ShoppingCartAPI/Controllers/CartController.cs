using Mango.Contracts.Responses;
using Mango.Services.ShoppingCartAPI.DTO;
using Mango.Services.ShoppingCartAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ShoppingCartAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> GetCart(string userId, CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetCartByUserIdAsync(userId, cancellationToken);
        if (cart == null)
            return Ok(ApiResponse<CartDto>.Success(new CartDto(0, userId, null, 0, 0, new List<CartItemDto>())));
        
        return Ok(ApiResponse<CartDto>.Success(cart));
    }

    [HttpPost("{userId}/items")]
    public async Task<ActionResult<ApiResponse<CartDto>>> AddToCart(
        string userId,
        [FromBody] AddCartItemDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var cart = await _cartService.AddToCartAsync(userId, dto, cancellationToken);
            return Ok(ApiResponse<CartDto>.Success(cart));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart");
            return BadRequest(ApiResponse<CartDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{userId}/items")]
    public async Task<ActionResult<ApiResponse<CartDto>>> UpdateCartItem(
        string userId,
        [FromBody] UpdateCartItemDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var cart = await _cartService.UpdateCartItemAsync(userId, dto, cancellationToken);
            return Ok(ApiResponse<CartDto>.Success(cart));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<CartDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{userId}/items/{cartItemId}")]
    public async Task<ActionResult<ApiResponse>> RemoveFromCart(
        string userId,
        int cartItemId,
        CancellationToken cancellationToken)
    {
        var result = await _cartService.RemoveFromCartAsync(userId, cartItemId, cancellationToken);
        if (!result)
            return NotFound(ApiResponse.Fail("Cart item not found"));
        
        return Ok(ApiResponse.Success(message: "Item removed from cart"));
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult<ApiResponse>> ClearCart(string userId, CancellationToken cancellationToken)
    {
        var result = await _cartService.ClearCartAsync(userId, cancellationToken);
        if (!result)
            return NotFound(ApiResponse.Fail("Cart not found"));
        
        return Ok(ApiResponse.Success(message: "Cart cleared"));
    }

    [HttpPost("{userId}/coupon")]
    public async Task<ActionResult<ApiResponse<CartDto>>> ApplyCoupon(
        string userId,
        [FromBody] ApplyCouponDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var cart = await _cartService.ApplyCouponAsync(userId, dto.CouponCode, cancellationToken);
            return Ok(ApiResponse<CartDto>.Success(cart));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<CartDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{userId}/coupon")]
    public async Task<ActionResult<ApiResponse<CartDto>>> RemoveCoupon(
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var cart = await _cartService.RemoveCouponAsync(userId, cancellationToken);
            return Ok(ApiResponse<CartDto>.Success(cart));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<CartDto>.Fail(ex.Message));
        }
    }

    [HttpPost("{userId}/checkout")]
    public async Task<ActionResult<ApiResponse>> Checkout(
        string userId,
        [FromQuery] int? rewardPointsUsed,
        CancellationToken cancellationToken)
    {
        var result = await _cartService.CheckoutAsync(userId, rewardPointsUsed, cancellationToken);
        if (!result)
            return BadRequest(ApiResponse.Fail("Cart is empty or checkout failed"));
        
        return Ok(ApiResponse.Success(message: "Checkout successful"));
    }
}
