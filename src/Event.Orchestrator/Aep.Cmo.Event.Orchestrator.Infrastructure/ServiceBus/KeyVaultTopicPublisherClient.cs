using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;
using Aep.Cmo.Shared.Contracts.Messaging;
using Aep.Cmo.Shared.ServiceBus.Interfaces;
using Aep.Cmo.Shared.ServiceBus.Messaging;

namespace Aep.Cmo.Event.Orchestrator.Infrastructure.ServiceBus;

/// <summary>
/// Publishes <see cref="KeyVaultMessage"/> messages to the configured Service Bus topic.
/// </summary>
public sealed class KeyVaultTopicPublisherClient
    : IKeyVaultTopicPublisherClient
{
    private readonly ITopicPublisherClient<KeyVaultMessage> _publisher;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyVaultTopicPublisherClient"/> class.
    /// </summary>
    public KeyVaultTopicPublisherClient(
        ITopicPublisherClient<KeyVaultMessage> publisher)
    {
        _publisher = publisher;
    }

    /// <inheritdoc />
    public Task PublishAsync(
        KeyVaultMessage payload,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var message = new TopicMessage<KeyVaultMessage>(
            payload,
            correlationId,
            DateTimeOffset.UtcNow);

        return _publisher.PublishAsync(message, cancellationToken);
    }
}
