using Aep.Cmo.Shared.KeyVault.Enums;

namespace Aep.Cmo.Shared.KeyVault.Interfaces;

public interface IKeyVaultAuthOptions
{
    AuthType AuthType { get; }
    string? TenantId { get; }
    string? ClientId { get; }
    string? ClientSecret { get; }
    string? ManagedIdentityClientId { get; }
}
