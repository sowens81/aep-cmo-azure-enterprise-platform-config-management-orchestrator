using ConfigManagement.Shared.ServiceBus.Models;

namespace ConfigManagement.Sync.Orchestrator.Domain.Models;

public sealed class EventMessage<TPayload> : BaseMessage<TPayload>
{
    public string EventGridId { get; init; } = default!;
}
