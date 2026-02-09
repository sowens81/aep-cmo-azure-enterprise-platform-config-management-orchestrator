namespace ConfigManagement.Shared.KeyVault.Options;

public sealed class KeyVaultOptions
{
    public string VaultUri { get; init; } = default!;
}

public interface IKeyVaultOptions
{
    string KeyVaultUri { get; }
}