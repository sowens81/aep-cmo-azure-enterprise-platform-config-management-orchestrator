namespace ConfigManagement.Shared.KeyVault.Authentication;

public sealed class KeyVaultAuthOptions
{
    public KeyVaultAuthType AuthType { get; init; } = KeyVaultAuthType.Default;
    public string? TenantId { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
    public string? ManagedIdentityClientId { get; init; }
}
