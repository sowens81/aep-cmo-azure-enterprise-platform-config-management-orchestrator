namespace ConfigManagement.Shared.ServiceBus.Models;

public sealed class ResultMessage<TPayload> : BaseMessage<TPayload>
{
    public string EventType { get; init; } = default!;
    public string Source { get; init; } = default!;
    public string Organisation { get; init; } = default!;
    public string Region { get; init; } = default!;
    public string EnvironmentTier { get; init; } = default!;
    public string EnvironmentName { get; init; } = default!;
    public string Status { get; init; } = default!;
    public string? Message { get; init; }
}
