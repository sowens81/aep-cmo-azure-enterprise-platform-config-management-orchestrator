namespace ConfigManagement.Shared.KeyVault.Interfaces;

public interface IKeyVaultSecretClient
{
    Task<bool> SecretExistsAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<(Uri? Id, string? Name, string? Value)?> GetSecretValueAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> CreateSecretAsync(
        string name,
        string value,
        CancellationToken cancellationToken = default);

    Task SetSecretAsync(
        string name,
        string value,
        CancellationToken cancellationToken = default);

    Task DeleteSecretAsync(
        string name,
        CancellationToken cancellationToken = default);
}
