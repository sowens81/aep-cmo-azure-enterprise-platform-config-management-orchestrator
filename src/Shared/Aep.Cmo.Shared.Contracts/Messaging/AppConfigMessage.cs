using Aep.Cmo.Shared.Contracts.Enum;

namespace Aep.Cmo.Shared.Contracts.Messaging;

/// <summary>
/// Represents a configuration synchronization message transported over the service bus.
/// </summary>
/// <param name="ConfigKeyName">
/// The unique configuration key identifier.
/// </param>
/// <param name="Type">
/// The type of configuration synchronization event.
/// </param>
/// <param name="SyncAction">
/// The synchronization action to perform.
/// </param>
public sealed record AppConfigMessage(
    string ConfigKeyName,
    ConfigSyncMessageType Type,
    SyncAction SyncAction
);
