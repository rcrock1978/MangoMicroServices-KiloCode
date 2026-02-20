using Mango.Contracts.Events;
using Mango.MessageBus;
using Mango.Services.ProductAPI.Domain;
using Mango.Services.ProductAPI.DTO;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ProductAPI.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync(CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateProductAsync(UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> UpdateStockAsync(int productId, int quantityChange, bool isRestock, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
}

public class ProductService : IProductService
{
    private readonly ProductDbContext _context;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ProductDbContext context, IMessageBus messageBus, ILogger<ProductService> logger)
    {
        _context = context;
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        return products.Select(p => new ProductDto(
            p.Id, p.Name, p.Price, p.Description, p.ImageUrl,
            p.CategoryId, p.Category?.Name, p.StockQuantity, p.IsActive));
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product == null) return null;

        return new ProductDto(
            product.Id, product.Name, product.Price, product.Description, product.ImageUrl,
            product.CategoryId, product.Category?.Name, product.StockQuantity, product.IsActive);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId,
            StockQuantity = dto.StockQuantity,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductName} created with ID {ProductId}", product.Name, product.Id);

        return new ProductDto(
            product.Id, product.Name, product.Price, product.Description, product.ImageUrl,
            product.CategoryId, null, product.StockQuantity, product.IsActive);
    }

    public async Task<ProductDto> UpdateProductAsync(UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync(new object[] { dto.Id }, cancellationToken);
        if (product == null)
            throw new InvalidOperationException("Product not found");

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Description = dto.Description;
        product.ImageUrl = dto.ImageUrl;
        product.CategoryId = dto.CategoryId;
        product.StockQuantity = dto.StockQuantity;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} updated", product.Id);

        return new ProductDto(
            product.Id, product.Name, product.Price, product.Description, product.ImageUrl,
            product.CategoryId, null, product.StockQuantity, product.IsActive);
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, cancellationToken);
        if (product == null) return false;

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} deactivated", id);
        return true;
    }

    public async Task<bool> UpdateStockAsync(int productId, int quantityChange, bool isRestock, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync(new object[] { productId }, cancellationToken);
        if (product == null) return false;

        if (isRestock)
        {
            product.StockQuantity += quantityChange;
        }
        else
        {
            if (product.StockQuantity < quantityChange)
                throw new InvalidOperationException("Insufficient stock");
            
            product.StockQuantity -= quantityChange;
        }

        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        // Publish inventory updated event
        var inventoryEvent = new InventoryUpdatedEvent(
            product.Id,
            product.Name,
            quantityChange,
            product.StockQuantity,
            isRestock,
            DateTime.UtcNow
        );
        
        await _messageBus.PublishAsync(inventoryEvent, cancellationToken);

        _logger.LogInformation("Product {ProductId} stock updated. New quantity: {Stock}", productId, product.StockQuantity);
        return true;
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _context.Categories.ToListAsync(cancellationToken);
        return categories.Select(c => new CategoryDto(c.Id, c.Name, c.Description));
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FindAsync(new object[] { id }, cancellationToken);
        if (category == null) return null;
        
        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category {CategoryName} created with ID {CategoryId}", category.Name, category.Id);

        return new CategoryDto(category.Id, category.Name, category.Description);
    }
}
