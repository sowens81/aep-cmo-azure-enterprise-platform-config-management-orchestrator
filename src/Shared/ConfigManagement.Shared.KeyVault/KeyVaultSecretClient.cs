using Azure;
using Azure.Security.KeyVault.Secrets;
using ConfigManagement.Shared.KeyVault.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Shared.KeyVault;

public abstract class KeyVaultSecretClient : IKeyVaultSecretClient
{
    private readonly SecretClient _client;
    private readonly ILogger<KeyVaultSecretClient> _logger;

    protected KeyVaultSecretClient(
        IKeyVaultOptions options,
        IKeyVaultCredentialFactory credentialFactory,
        ILogger<KeyVaultSecretClient> logger)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint))
            throw new ArgumentException("VaultUri is required.", nameof(options.Endpoint));

        _client = new SecretClient(
            new Uri(options.Endpoint),
            credentialFactory.CreateCredential());

        _logger = logger;
    }

    public async Task<bool> SecretExistsAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
            var exists = await GetSecretValueAsync(
                name,
                cancellationToken: cancellationToken);

            if (exists == null)
            {
                _logger.LogInformation(
                    "Key Vault secret {Name} does not exist.",
                    name);
                return false;
            } 
            return true;
    }

    public async Task<(Uri? Id, string? Name, string? Value)?> GetSecretValueAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var secret = await _client.GetSecretAsync(
                name,
                cancellationToken: cancellationToken);

            return (secret.Value.Id, secret.Value.Name, secret.Value.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return (null, null, null);
        }
    }

    public async Task<bool> CreateSecretAsync(
        string name,
        string value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.SetSecretAsync(
                new KeyVaultSecret(name, value),
                cancellationToken);

            _logger.LogInformation(
                "Created Key Vault secret {Name}",
                name);

            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            // Secret already exists
            return false;
        }
    }

    public async Task SetSecretAsync(
        string name,
        string value,
        CancellationToken cancellationToken = default)
    {
        await _client.SetSecretAsync(
            new KeyVaultSecret(name, value),
            cancellationToken);

        _logger.LogInformation(
            "Set Key Vault secret {Name}",
            name);
    }

    public async Task DeleteSecretAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        await _client.StartDeleteSecretAsync(
            name,
            cancellationToken);

        _logger.LogInformation(
            "Deleted Key Vault secret {Name}",
            name);
    }
}
