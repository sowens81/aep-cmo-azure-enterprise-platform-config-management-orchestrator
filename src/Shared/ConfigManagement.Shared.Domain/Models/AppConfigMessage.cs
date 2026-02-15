using ConfigManagement.Shared.Domain.Enum;

namespace ConfigManagement.Shared.Domain.Models;

public sealed class AppConfigMessage
{
    public required string ConfigKeyId { get; init; }
    public required string ConfigKeyName { get; init; }
    public required ConfigSyncMessageType Type { get; init; }
    public required SyncAction SyncAction { get; init; }
}
