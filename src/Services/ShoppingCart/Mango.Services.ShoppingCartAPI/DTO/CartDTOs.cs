namespace Mango.Services.ShoppingCartAPI.DTO;

public record CartDto(
    int Id,
    string UserId,
    string? CouponCode,
    decimal Discount,
    decimal Total,
    List<CartItemDto> Items
);

public record CartItemDto(
    int Id,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    decimal Total
);

public record AddCartItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal Price
);

public record UpdateCartItemDto(
    int CartItemId,
    int Quantity
);

public record ApplyCouponDto(
    string CouponCode
);
