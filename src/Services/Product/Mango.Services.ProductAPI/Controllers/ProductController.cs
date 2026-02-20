using Mango.Contracts.Responses;
using Mango.Services.ProductAPI.DTO;
using Mango.Services.ProductAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ProductAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProducts(CancellationToken cancellationToken)
    {
        var products = await _productService.GetProductsAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.Success(products));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
            return NotFound(ApiResponse<ProductDto>.Fail("Product not found"));
        
        return Ok(ApiResponse<ProductDto>.Success(product));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct(
        [FromBody] CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.CreateProductAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, 
                ApiResponse<ProductDto>.Success(product, 201));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return BadRequest(ApiResponse<ProductDto>.Fail(ex.Message));
        }
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(
        [FromBody] UpdateProductDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.UpdateProductAsync(dto, cancellationToken);
            return Ok(ApiResponse<ProductDto>.Success(product));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<ProductDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product");
            return BadRequest(ApiResponse<ProductDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteProductAsync(id, cancellationToken);
        if (!result)
            return NotFound(ApiResponse.Fail("Product not found"));
        
        return Ok(ApiResponse.Success(message: "Product deleted successfully"));
    }

    [HttpPut("{id}/stock")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> UpdateStock(
        int id,
        [FromQuery] int quantity,
        [FromQuery] bool isRestock,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _productService.UpdateStockAsync(id, quantity, isRestock, cancellationToken);
            if (!result)
                return NotFound(ApiResponse.Fail("Product not found"));
            
            return Ok(ApiResponse.Success(message: "Stock updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}
