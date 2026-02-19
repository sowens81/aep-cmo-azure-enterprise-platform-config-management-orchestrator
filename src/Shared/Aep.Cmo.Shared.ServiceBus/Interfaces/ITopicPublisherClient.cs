using Aep.Cmo.Shared.ServiceBus.Messaging;

namespace Aep.Cmo.Shared.ServiceBus.Interfaces;

/// <summary>
/// Defines a contract for publishing strongly-typed messages
/// to an Azure Service Bus topic.
/// </summary>
/// <typeparam name="TPayload">
/// The type of the message payload.
/// </typeparam>
public interface ITopicPublisherClient<TPayload>
{
    /// <summary>
    /// Publishes the specified message to the configured topic.
    /// </summary>
    /// <param name="message">
    /// The message envelope containing the payload and metadata.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token used to cancel the operation.
    /// </param>
    Task PublishAsync(
        TopicMessage<TPayload> message,
        CancellationToken cancellationToken = default);
}
