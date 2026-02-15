using ConfigManagement.Shared.ServiceBus.Models;

namespace ConfigManagement.Shared.Domain.Models;

public sealed class EventMessage<TPayload> : BaseMessage<TPayload>
{
    public string EventGridId { get; init; } = default!;
}
