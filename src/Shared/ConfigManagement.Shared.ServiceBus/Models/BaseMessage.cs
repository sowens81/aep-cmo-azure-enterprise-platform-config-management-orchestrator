namespace ConfigManagement.Shared.ServiceBus.Models;

public class BaseMessage<TPayload>
{
    public string ServiceName { get; init; } = default!;
    public string CorrelationId { get; init; } = default!;
    public string TraceId { get; init; } = default!;
    public string? SpanId { get; init; }
    public TPayload Payload { get; init; } = default!;
    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;
}
