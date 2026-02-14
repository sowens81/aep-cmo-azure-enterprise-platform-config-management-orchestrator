
namespace ConfigManagement.Shared.ServiceBus.Models;


/// <summary>
/// Represents a base wrapper for messages published to or consumed from the Service Bus.
/// </summary>
/// <typeparam name="TPayload">
/// The type of the message payload being transported.
/// </typeparam>
/// <remarks>
/// This class provides common metadata used for distributed tracing and correlation:
/// <list type="bullet">
/// <item>
/// <description><see cref="CorrelationId"/> – Used to correlate related messages and operations across services.</description>
/// </item>
/// <item>
/// <description><see cref="Traceparent"/> – The full W3C traceparent header value (<c>00-traceid-spanid-flags</c>) used for distributed tracing.</description>
/// </item>
/// <item>
/// <description><see cref="TimestampUtc"/> – The UTC timestamp when the message instance was created.</description>
/// </item>
/// </list>
/// 
/// All properties use <c>init</c> accessors to ensure immutability after object initialization.
/// </remarks>
public class BaseMessage<TPayload>
{
/// <summary>
/// Gets the correlation identifier used to associate related messages
/// across different services and operations.
/// </summary>
public string CorrelationId { get; init; } = default!;

/// <summary>
/// Gets the full W3C <c>traceparent</c> header value
/// in the format <c>00-traceid-spanid-flags</c>.
/// </summary>
/// <remarks>
/// This value should be propagated across service boundaries to enable
/// end-to-end distributed tracing. The value may be <c>null</c> if tracing
/// is not enabled or not available at the time of message creation.
/// </remarks>
public string? Traceparent { get; init; }

/// <summary>
/// Gets the strongly-typed payload carried by this message.
/// </summary>
public TPayload Payload { get; init; } = default!;

/// <summary>
/// Gets the UTC timestamp indicating when the message instance was created.
/// Defaults to <see cref="DateTimeOffset.UtcNow"/>.
/// </summary>
public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;

/// <summary>
/// Initializes a new instance of the <see cref="BaseMessage{TPayload}"/> class.
/// </summary>
/// <remarks>
/// Required properties should be set using object initializer syntax.
/// </remarks>
public BaseMessage() { }

/// <summary>
/// Initializes a new instance of the <see cref="BaseMessage{TPayload}"/> class
/// with the specified correlation identifier and payload.
/// </summary>
/// <param name="correlationId">
/// The correlation identifier used to link related operations.
/// </param>
/// <param name="payload">
/// The strongly-typed payload to be transported within the message.
/// </param>
/// <remarks>
/// The <see cref="TimestampUtc"/> property is automatically set to the current UTC time.
/// The <see cref="Traceparent"/> property can be set separately if distributed tracing is required.
/// </remarks>
public BaseMessage(string correlationId, TPayload payload)
{
    CorrelationId = correlationId;
    Payload = payload;
}
}