using Aep.Cmo.Shared.Domain.Models;
using Aep.Cmo.Shared.ServiceBus.Interfaces;
using Aep.Cmo.Shared.ServiceBus.Models;

namespace Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;

public interface IKeyVaultTopicPublisherClient<TPayload>
    : ITopicPublisherClient<SyncMessage<TPayload>, TPayload>
{
}
