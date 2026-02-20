using Mango.Contracts.Events;
using Mango.MessageBus;
using Mango.Services.OrderAPI.Domain;
using Mango.Services.OrderAPI.DTO;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.OrderAPI.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateOrderStatusAsync(int orderId, string status, CancellationToken cancellationToken = default);
    Task<bool> ProcessPaymentAsync(int orderId, string paymentId, CancellationToken cancellationToken = default);
}

public class OrderService : IOrderService
{
    private readonly OrderDbContext _context;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<OrderService> _logger;

    public OrderService(OrderDbContext context, IMessageBus messageBus, ILogger<OrderService> logger)
    {
        _context = context;
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders.Include(o => o.Items).Where(o => o.UserId == userId).ToListAsync(cancellationToken);
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        return order == null ? null : MapToDto(order);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        var order = new Order
        {
            UserId = dto.UserId,
            UserEmail = dto.UserEmail,
            UserName = dto.UserName,
            TotalAmount = dto.TotalAmount,
            CouponCode = dto.CouponCode,
            Discount = dto.Discount,
            Status = OrderStatus.Pending,
            Items = dto.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                Price = i.Price,
                Total = i.Total
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish OrderPlaced event
        var orderPlacedEvent = new OrderPlacedEvent(
            Guid.NewGuid(),
            order.UserId,
            order.UserEmail,
            order.UserName,
            order.TotalAmount,
            DateTime.UtcNow,
            order.Items.Select(i => new OrderItemEvent(i.ProductId, i.ProductName, i.Quantity, i.Price, i.Total)).ToList()
        );
        await _messageBus.PublishAsync(orderPlacedEvent, cancellationToken);

        _logger.LogInformation("Order {OrderId} created for user {UserId}", order.Id, order.UserId);
        return MapToDto(order);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, string status, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FindAsync(new object[] { orderId }, cancellationToken)
            ?? throw new InvalidOperationException("Order not found");

        order.Status = Enum.Parse<OrderStatus>(status);
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, status);
        return MapToDto(order);
    }

    public async Task<bool> ProcessPaymentAsync(int orderId, string paymentId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FindAsync(new object[] { orderId }, cancellationToken);
        if (order == null) return false;

        order.PaymentId = paymentId;
        order.PaidAt = DateTime.UtcNow;
        order.Status = OrderStatus.Processing;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        // Publish PaymentCompleted event
        var paymentEvent = new PaymentCompletedEvent(
            Guid.NewGuid(),
            paymentId,
            order.UserId,
            order.TotalAmount,
            true,
            DateTime.UtcNow
        );
        await _messageBus.PublishAsync(paymentEvent, cancellationToken);

        _logger.LogInformation("Payment processed for order {OrderId}", orderId);
        return true;
    }

    private static OrderDto MapToDto(Order o) => new(
        o.Id, o.UserId, o.UserEmail, o.UserName, o.TotalAmount, o.CouponCode, o.Discount,
        o.Status.ToString(), o.PaymentId, o.PaidAt, o.CreatedAt,
        o.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.ProductName, i.Quantity, i.Price, i.Total)).ToList()
    );
}
