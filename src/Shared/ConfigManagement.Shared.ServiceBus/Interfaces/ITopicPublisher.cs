namespace ConfigManagement.Shared.ServiceBus.Interfaces;

public interface ITopicPublisher<in TMessage>
{
    Task PublishAsync(
        TMessage message,
        CancellationToken cancellationToken = default
    );
}
