using Aep.Cmo.Shared.ServiceBus.Messaging;

namespace Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;

/// <summary>
/// Defines a contract for publishing Key Vault synchronization messages.
/// </summary>
public interface IKeyVaultTopicPublisherClient
{
    /// <summary>
    /// Publishes the specified <see cref="KeyVaultMessage"/> to the topic.
    /// </summary>
    Task PublishAsync(
        KeyVaultMessage payload,
        string correlationId,
        CancellationToken cancellationToken = default);
}
