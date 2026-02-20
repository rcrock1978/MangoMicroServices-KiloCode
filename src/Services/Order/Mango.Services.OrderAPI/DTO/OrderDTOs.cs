namespace Mango.Services.OrderAPI.DTO;

public record OrderDto(
    int Id,
    string UserId,
    string UserEmail,
    string UserName,
    decimal TotalAmount,
    string? CouponCode,
    decimal Discount,
    string Status,
    string? PaymentId,
    DateTime? PaidAt,
    DateTime CreatedAt,
    List<OrderItemDto> Items
);

public record OrderItemDto(
    int Id,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    decimal Total
);

public record CreateOrderDto(
    string UserId,
    string UserEmail,
    string UserName,
    decimal TotalAmount,
    string? CouponCode,
    decimal Discount,
    List<CreateOrderItemDto> Items
);

public record CreateOrderItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    decimal Total
);
