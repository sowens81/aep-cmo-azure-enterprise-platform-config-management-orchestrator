using Aep.Cmo.Shared.KeyVault.Enums;
using Aep.Cmo.Shared.KeyVault.Interfaces;

namespace Aep.Cmo.Shared.KeyVault.Options;

public sealed class KeyVaultAuthOptions : IKeyVaultAuthOptions
{
    public AuthType AuthType { get; init; } = AuthType.Default;
    public string? TenantId { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
    public string? ManagedIdentityClientId { get; init; }
}
