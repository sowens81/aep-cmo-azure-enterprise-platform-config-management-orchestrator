namespace ConfigManagement.Shared.ServiceBus.Interfaces;

/// <summary>
/// Defines a contract for publishing messages to a topic.
/// </summary>
/// <typeparam name="TMessage">
/// The type of message to be published.
/// </typeparam>
/// <remarks>
/// Implementations of this interface are responsible for serializing and
/// sending messages to the underlying messaging infrastructure
/// (e.g., Azure Service Bus topic).
///
/// The generic type parameter is declared as <c>in</c> (contravariant),
/// allowing a publisher of a base type to be used where a publisher of a
/// derived type is expected.
/// </remarks>
public interface ITopicPublisher<in TMessage>
{
    /// <summary>
    /// Publishes a message asynchronously to the configured topic.
    /// </summary>
    /// <param name="message">
    /// The message instance to publish.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous publish operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="message"/> is <c>null</c>.
    /// </exception>
    Task PublishAsync(
        TMessage message,
        CancellationToken cancellationToken = default
    );
}
