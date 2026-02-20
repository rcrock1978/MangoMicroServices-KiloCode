using Mango.Contracts.Events;
using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Domain;
using Mango.Services.ShoppingCartAPI.DTO;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Services;

public interface ICartService
{
    Task<CartDto?> GetCartByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<CartDto> AddToCartAsync(string userId, AddCartItemDto dto, CancellationToken cancellationToken = default);
    Task<CartDto> UpdateCartItemAsync(string userId, UpdateCartItemDto dto, CancellationToken cancellationToken = default);
    Task<bool> RemoveFromCartAsync(string userId, int cartItemId, CancellationToken cancellationToken = default);
    Task<bool> ClearCartAsync(string userId, CancellationToken cancellationToken = default);
    Task<CartDto> ApplyCouponAsync(string userId, string couponCode, CancellationToken cancellationToken = default);
    Task<CartDto> RemoveCouponAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> CheckoutAsync(string userId, int? rewardPointsUsed, CancellationToken cancellationToken = default);
}

public class CartService : ICartService
{
    private readonly CartDbContext _context;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<CartService> _logger;

    public CartService(CartDbContext context, IMessageBus messageBus, ILogger<CartService> logger)
    {
        _context = context;
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task<CartDto?> GetCartByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null) return null;

        return MapToDto(cart);
    }

    public async Task<CartDto> AddToCartAsync(string userId, AddCartItemDto dto, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
        }

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += dto.Quantity;
            existingItem.Total = existingItem.Quantity * existingItem.Price;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                Quantity = dto.Quantity,
                Price = dto.Price,
                Total = dto.Quantity * dto.Price
            });
        }

        cart.Total = cart.Items.Sum(i => i.Total) - cart.Discount;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added product {ProductId} to cart for user {UserId}", dto.ProductId, userId);

        return MapToDto(cart);
    }

    public async Task<CartDto> UpdateCartItemAsync(string userId, UpdateCartItemDto dto, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Cart not found");

        var item = cart.Items.FirstOrDefault(i => i.Id == dto.CartItemId)
            ?? throw new InvalidOperationException("Cart item not found");

        item.Quantity = dto.Quantity;
        item.Total = item.Quantity * item.Price;
        cart.Total = cart.Items.Sum(i => i.Total) - cart.Discount;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated cart item {CartItemId} for user {UserId}", dto.CartItemId, userId);

        return MapToDto(cart);
    }

    public async Task<bool> RemoveFromCartAsync(string userId, int cartItemId, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null) return false;

        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item == null) return false;

        cart.Items.Remove(item);
        cart.Total = cart.Items.Sum(i => i.Total) - cart.Discount;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Removed cart item {CartItemId} from cart for user {UserId}", cartItemId, userId);

        return true;
    }

    public async Task<bool> ClearCartAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null) return false;

        cart.Items.Clear();
        cart.Total = 0;
        cart.Discount = 0;
        cart.CouponCode = null;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cleared cart for user {UserId}", userId);

        return true;
    }

    public async Task<CartDto> ApplyCouponAsync(string userId, string couponCode, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Cart not found");

        // Apply coupon discount (simplified - in real app would call CouponAPI)
        cart.CouponCode = couponCode;
        cart.Discount = cart.Items.Sum(i => i.Total) * 0.10m; // 10% discount
        cart.Total = cart.Items.Sum(i => i.Total) - cart.Discount;

        await _context.SaveChangesAsync(cancellationToken);

        // Publish coupon applied event
        var couponEvent = new CouponAppliedEvent(
            couponCode,
            userId,
            cart.Discount,
            cart.Items.Sum(i => i.Total),
            cart.Total,
            DateTime.UtcNow
        );
        await _messageBus.PublishAsync(couponEvent, cancellationToken);

        _logger.LogInformation("Applied coupon {CouponCode} to cart for user {UserId}", couponCode, userId);

        return MapToDto(cart);
    }

    public async Task<CartDto> RemoveCouponAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Cart not found");

        cart.CouponCode = null;
        cart.Discount = 0;
        cart.Total = cart.Items.Sum(i => i.Total);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Removed coupon from cart for user {UserId}", userId);

        return MapToDto(cart);
    }

    public async Task<bool> CheckoutAsync(string userId, int? rewardPointsUsed, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null || !cart.Items.Any()) return false;

        // Publish CartCheckedOut event for OrderAPI to consume
        var checkoutEvent = new CartCheckedOutEvent(
            userId,
            Guid.NewGuid(),
            cart.Items.Select(i => new CartItemEvent(
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.Price,
                i.Total
            )).ToList(),
            cart.Total,
            cart.CouponCode,
            rewardPointsUsed,
            DateTime.UtcNow
        );

        await _messageBus.PublishAsync(checkoutEvent, cancellationToken);

        // Clear cart after checkout
        cart.Items.Clear();
        cart.Total = 0;
        cart.Discount = 0;
        cart.CouponCode = null;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cart checked out for user {UserId}", userId);

        return true;
    }

    private static CartDto MapToDto(Cart cart) => new(
        cart.Id,
        cart.UserId,
        cart.CouponCode,
        cart.Discount,
        cart.Total,
        cart.Items.Select(i => new CartItemDto(i.Id, i.ProductId, i.ProductName, i.Quantity, i.Price, i.Total)).ToList()
    );
}
