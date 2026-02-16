using Aep.Cmo.Shared.Domain.Enum;

namespace Aep.Cmo.Shared.Domain.Models;

public sealed class KeyVaultMessage
{
    public required string SecretId { get; init; }
    public required string SecretName { get; init; }
    public required SyncAction SyncAction { get; init; }

}
