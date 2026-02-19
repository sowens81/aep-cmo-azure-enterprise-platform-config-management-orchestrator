using Aep.Cmo.Shared.Contracts.Enum;

namespace Aep.Cmo.Shared.ServiceBus.Messaging;

/// <summary>
/// Represents a Key Vault synchronization message transported between services.
/// </summary>
/// <param name="SecretId">
/// The unique identifier of the secret.
/// </param>
/// <param name="SecretName">
/// The name of the secret.
/// </param>
/// <param name="SyncAction">
/// The synchronization action to perform.
/// </param>
public sealed record KeyVaultMessage(
    string SecretId,
    string SecretName,
    SyncAction SyncAction
);
