using ConfigManagement.Shared.ServiceBus.Models;

namespace ConfigManagement.Shared.ServiceBus;

public interface IResultPublisher
{
    Task PublishAsync<TPayload>(
        ResultMessage<TPayload> message,
        CancellationToken cancellationToken);
}
