using Aep.Cmo.Shared.Domain.Models;
using Aep.Cmo.Shared.ServiceBus.Interfaces;

namespace Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;

public interface IAppConfigTopicPublisherClient<TPayload>
    : ITopicPublisherClient<SyncMessage<TPayload>, TPayload>
{
}
