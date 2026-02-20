using Mango.SharedKernel;

namespace Mango.Services.OrderAPI.Domain;

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public class Order : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? CouponCode { get; set; }
    public decimal Discount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? PaymentId { get; set; }
    public DateTime? PaidAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
}
