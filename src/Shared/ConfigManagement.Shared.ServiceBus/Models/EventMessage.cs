namespace ConfigManagement.Shared.ServiceBus.Models;

public sealed class EventMessage<TPayload>
{
    public string EventType { get; init; } = default!;
    public string Source { get; init; } = default!;
    public string EnvironmentName { get; init; } = default!;
    public string CorrelationId { get; init; } = default!;
    public string TraceId { get; init; } = default!;
    public string? SpanId { get; init; }
    public TPayload Payload { get; init; } = default!;
    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;
}
