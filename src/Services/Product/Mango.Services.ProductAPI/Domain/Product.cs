using Mango.SharedKernel;

namespace Mango.Services.ProductAPI.Domain;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
