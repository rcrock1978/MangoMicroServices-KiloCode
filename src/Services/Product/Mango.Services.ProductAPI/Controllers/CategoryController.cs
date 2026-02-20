using Mango.Contracts.Responses;
using Mango.Services.ProductAPI.DTO;
using Mango.Services.ProductAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ProductAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(IProductService productService, ILogger<CategoryController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _productService.GetCategoriesAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<CategoryDto>>.Success(categories));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id, CancellationToken cancellationToken)
    {
        var category = await _productService.GetCategoryByIdAsync(id, cancellationToken);
        if (category == null)
            return NotFound(ApiResponse<CategoryDto>.Fail("Category not found"));
        
        return Ok(ApiResponse<CategoryDto>.Success(category));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory(
        [FromBody] CreateCategoryDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var category = await _productService.CreateCategoryAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, 
                ApiResponse<CategoryDto>.Success(category, 201));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return BadRequest(ApiResponse<CategoryDto>.Fail(ex.Message));
        }
    }
}
