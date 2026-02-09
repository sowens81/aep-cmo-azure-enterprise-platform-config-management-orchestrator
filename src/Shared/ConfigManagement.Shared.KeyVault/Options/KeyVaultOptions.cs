using ConfigManagement.Shared.KeyVault.Interfaces;

namespace ConfigManagement.Shared.KeyVault.Options;

public sealed class KeyVaultOptions : IKeyVaultOptions
{
    public string Endpoint { get; init; } = default!;
}
