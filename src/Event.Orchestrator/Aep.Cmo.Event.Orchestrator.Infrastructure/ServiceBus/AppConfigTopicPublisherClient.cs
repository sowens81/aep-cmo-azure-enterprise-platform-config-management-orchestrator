using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;
using Aep.Cmo.Shared.Contracts.Messaging;
using Aep.Cmo.Shared.ServiceBus.Interfaces;
using Aep.Cmo.Shared.ServiceBus.Messaging;
using Microsoft.Extensions.Logging;

namespace Aep.Cmo.Event.Orchestrator.Infrastructure.ServiceBus;

/// <summary>
/// Publishes <see cref="AppConfigMessage"/> messages to the configured Service Bus topic.
/// </summary>
public sealed class AppConfigTopicPublisherClient
    : IAppConfigTopicPublisherClient
{
    private readonly ITopicPublisherClient<AppConfigMessage> _publisher;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppConfigTopicPublisherClient"/> class.
    /// </summary>
    public AppConfigTopicPublisherClient(
        ITopicPublisherClient<AppConfigMessage> publisher)
    {
        _publisher = publisher;
    }

    /// <inheritdoc />
    public Task PublishAsync(
        AppConfigMessage payload,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var message = new TopicMessage<AppConfigMessage>(
            payload,
            correlationId,
            DateTimeOffset.UtcNow);

        return _publisher.PublishAsync(message, cancellationToken);
    }
}
