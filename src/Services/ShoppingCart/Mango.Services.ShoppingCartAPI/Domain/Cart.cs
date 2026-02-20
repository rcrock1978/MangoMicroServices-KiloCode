using Mango.SharedKernel;

namespace Mango.Services.ShoppingCartAPI.Domain;

public class Cart : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public List<CartItem> Items { get; set; } = new();
}

public class CartItem : BaseEntity
{
    public int CartId { get; set; }
    public Cart? Cart { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
}
