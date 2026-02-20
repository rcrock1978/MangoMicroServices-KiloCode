using Mango.Contracts.Responses;
using Mango.Services.OrderAPI.DTO;
using Mango.Services.OrderAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.OrderAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetUserOrders(string userId, CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetOrdersByUserIdAsync(userId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<OrderDto>>.Success(orders));
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder(int orderId, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId, cancellationToken);
        if (order == null)
            return NotFound(ApiResponse<OrderDto>.Fail("Order not found"));
        return Ok(ApiResponse<OrderDto>.Success(order));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetOrder), new { orderId = order.Id }, ApiResponse<OrderDto>.Success(order, 201));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return BadRequest(ApiResponse<OrderDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{orderId}/status")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateStatus(int orderId, [FromQuery] string status, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(orderId, status, cancellationToken);
            return Ok(ApiResponse<OrderDto>.Success(order));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<OrderDto>.Fail(ex.Message));
        }
    }

    [HttpPost("{orderId}/payment")]
    public async Task<ActionResult<ApiResponse>> ProcessPayment(int orderId, [FromQuery] string paymentId, CancellationToken cancellationToken)
    {
        var result = await _orderService.ProcessPaymentAsync(orderId, paymentId, cancellationToken);
        if (!result)
            return NotFound(ApiResponse.Fail("Order not found"));
        return Ok(ApiResponse.Success(message: "Payment processed successfully"));
    }
}
