namespace Mango.Services.ProductAPI.DTO;

public record ProductDto(
    int Id,
    string Name,
    decimal Price,
    string? Description,
    string? ImageUrl,
    int CategoryId,
    string? CategoryName,
    int StockQuantity,
    bool IsActive
);

public record CreateProductDto(
    string Name,
    decimal Price,
    string? Description,
    string? ImageUrl,
    int CategoryId,
    int StockQuantity
);

public record UpdateProductDto(
    int Id,
    string Name,
    decimal Price,
    string? Description,
    string? ImageUrl,
    int CategoryId,
    int StockQuantity,
    bool IsActive
);

public record CategoryDto(
    int Id,
    string Name,
    string? Description
);

public record CreateCategoryDto(
    string Name,
    string? Description
);
