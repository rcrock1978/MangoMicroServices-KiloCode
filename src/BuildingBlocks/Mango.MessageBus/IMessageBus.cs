namespace Mango.MessageBus;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
    Task PublishAsync<T>(T message, string topicOrQueueName, CancellationToken cancellationToken = default) where T : class;
}
