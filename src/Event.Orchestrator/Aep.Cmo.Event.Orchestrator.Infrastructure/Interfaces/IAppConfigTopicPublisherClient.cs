using Aep.Cmo.Shared.Contracts.Messaging;

namespace Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;

/// <summary>
/// Defines a contract for publishing App Configuration synchronization messages.
/// </summary>
public interface IAppConfigTopicPublisherClient
{
    /// <summary>
    /// Publishes the specified <see cref="AppConfigMessage"/> to the topic.
    /// </summary>
    Task PublishAsync(
        AppConfigMessage payload,
        string correlationId,
        CancellationToken cancellationToken = default);
}
