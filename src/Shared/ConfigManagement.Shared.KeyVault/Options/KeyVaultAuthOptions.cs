using ConfigManagement.Shared.KeyVault.Enums;
using ConfigManagement.Shared.KeyVault.Interfaces;

namespace ConfigManagement.Shared.KeyVault.Options;

public sealed class KeyVaultAuthOptions : IKeyVaultAuthOptions
{
    public AuthType AuthType { get; init; } = AuthType.Default;
    public string? TenantId { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
    public string? ManagedIdentityClientId { get; init; }
}
