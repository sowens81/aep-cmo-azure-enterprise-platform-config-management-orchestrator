using Aep.Cmo.Shared.KeyVault.Interfaces;

namespace Aep.Cmo.Shared.KeyVault.Options;

public sealed class KeyVaultOptions : IKeyVaultOptions
{
    public string Endpoint { get; init; } = default!;
}
