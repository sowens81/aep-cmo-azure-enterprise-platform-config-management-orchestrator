using Aep.Cmo.Shared.ServiceBus.Models;

namespace Aep.Cmo.Shared.ServiceBus.Interfaces;

public interface ITopicPublisherClient<TMessage, TPayload>
    where TMessage : BaseMessage<TPayload>
{
    Task PublishAsync(
        TMessage message,
        CancellationToken cancellationToken = default);
}
