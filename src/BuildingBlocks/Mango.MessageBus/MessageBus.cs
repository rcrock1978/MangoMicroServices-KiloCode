using MassTransit;
using Microsoft.Extensions.Logging;

namespace Mango.MessageBus;

public class MessageBus : IMessageBus
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MessageBus> _logger;

    public MessageBus(IPublishEndpoint publishEndpoint, ILogger<MessageBus> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogInformation("Publishing message of type {MessageType}", typeof(T).Name);
        await _publishEndpoint.Publish(message, cancellationToken);
        _logger.LogInformation("Successfully published message of type {MessageType}", typeof(T).Name);
    }

    public async Task PublishAsync<T>(T message, string topicOrQueueName, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogInformation("Publishing message of type {MessageType} to {TopicOrQueueName}", 
            typeof(T).Name, topicOrQueueName);
        
        // Use publish for now - in production, would use proper topic/queue configuration
        await _publishEndpoint.Publish(message, cancellationToken);
        
        _logger.LogInformation("Successfully published message of type {MessageType} to {TopicOrQueueName}", 
            typeof(T).Name, topicOrQueueName);
    }
}
