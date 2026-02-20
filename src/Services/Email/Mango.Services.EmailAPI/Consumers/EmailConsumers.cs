using Mango.Contracts.Events;
using MassTransit;

namespace Mango.Services.EmailAPI.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Sending welcome email to {Email}", message.Email);
        // In real implementation, integrate with email service (SendGrid, etc.)
        await Task.Delay(100);
        _logger.LogInformation("Welcome email sent to {Email}", message.Email);
    }
}

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Sending order confirmation email to {Email} for order {OrderId}", message.UserEmail, message.OrderId);
        // In real implementation, integrate with email service
        await Task.Delay(100);
        _logger.LogInformation("Order confirmation email sent to {Email}", message.UserEmail);
    }
}
