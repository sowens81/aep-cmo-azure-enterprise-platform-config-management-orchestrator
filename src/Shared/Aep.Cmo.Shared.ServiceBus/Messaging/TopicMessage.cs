namespace Aep.Cmo.Shared.ServiceBus.Messaging;

/// <summary>
/// Represents a generic integration message envelope used for cross-service communication.
/// </summary>
/// <typeparam name="TPayload">
/// The strongly-typed payload carried by the message.
/// </typeparam>
/// <param name="Payload">
/// The message payload.
/// </param>
/// <param name="CorrelationId">
/// The identifier used to correlate related operations across services.
/// </param>
/// <param name="TimestampUtc">
/// The UTC timestamp indicating when the message was created.
/// </param>
public record TopicMessage<TPayload>(
    TPayload Payload,
    string CorrelationId,
    DateTimeOffset TimestampUtc
);
