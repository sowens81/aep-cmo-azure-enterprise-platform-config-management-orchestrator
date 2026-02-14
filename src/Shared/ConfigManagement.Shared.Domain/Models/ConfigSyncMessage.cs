using ConfigManagement.Shared.Domain.Enum;

namespace ConfigManagement.Shared.Domain.Models;

public sealed class ConfigSyncMessage
{
    public string Key { get; init; } = default!;
    public ConfigSyncMessageType Type { get; init; } = default!;
    public string? KeyVaultSecretUri { get; init; }
    public SyncAction SyncAction { get; init; }
}
