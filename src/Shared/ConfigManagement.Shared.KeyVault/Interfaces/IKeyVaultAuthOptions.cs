using ConfigManagement.Shared.KeyVault.Enums;

namespace ConfigManagement.Shared.KeyVault.Interfaces;

public interface IKeyVaultAuthOptions
{
    AuthType AuthType { get; }
    string? TenantId { get; }
    string? ClientId { get; }
    string? ClientSecret { get; }
    string? ManagedIdentityClientId { get; }
}
