namespace ConfigManagement.Shared.ServiceBus.Models;

public sealed class EventMessage<TPayload> : BaseMessage<TPayload>
{
    public string EventType { get; init; } = default!;
    public string Source { get; init; } = default!;
    public string EnvironmentName { get; init; } = default!;
}
