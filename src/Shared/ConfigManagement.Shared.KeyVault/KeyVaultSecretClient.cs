using Azure;
using Azure.Security.KeyVault.Secrets;
using ConfigManagement.Shared.KeyVault.Interfaces;
using ConfigManagement.Shared.KeyVault.OpenTelemetry;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ConfigManagement.Shared.KeyVault;

/// <summary>
/// Base implementation for interacting with Azure Key Vault secrets.
/// </summary>
/// <remarks>
/// Each operation creates an OpenTelemetry span (<see cref="ActivityKind.Client"/>)
/// to enable distributed tracing and dependency visibility.
/// Secret values are never logged.
/// </remarks>
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

        if (credentialFactory is null)
            throw new ArgumentNullException(nameof(credentialFactory));

        _client = new SecretClient(
            new Uri(options.Endpoint),
            credentialFactory.CreateCredential());

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> SecretExistsAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Source.StartActivity(
            "KeyVault Secret Exists",
            ActivityKind.Client);

        activity?.SetTag("kv.operation", "exists");
        activity?.SetTag("kv.secret.name", name);

        var secret = await GetSecretValueAsync(name, cancellationToken);

        if (secret == null || secret.Value.Name == null)
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
        using var activity = Telemetry.Source.StartActivity(
            "KeyVault Get Secret",
            ActivityKind.Client);

        activity?.SetTag("kv.operation", "get");
        activity?.SetTag("kv.secret.name", name);

        try
        {
            var secret = await _client.GetSecretAsync(
                name,
                cancellationToken: cancellationToken);

            activity?.SetTag("kv.secret.id", secret.Value.Id.ToString());

            return (secret.Value.Id, secret.Value.Name, secret.Value.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Secret not found");
            return null;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Failed to retrieve Key Vault secret {Name}",
                name);

            throw;
        }
    }

    public async Task<bool> CreateSecretAsync(
        string name,
        string value,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Source.StartActivity(
            "KeyVault Create Secret",
            ActivityKind.Client);

        activity?.SetTag("kv.operation", "create");
        activity?.SetTag("kv.secret.name", name);

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
            activity?.SetStatus(ActivityStatusCode.Error, "Secret already exists");
            return false;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Failed to create Key Vault secret {Name}",
                name);

            throw;
        }
    }

    public async Task SetSecretAsync(
        string name,
        string value,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Source.StartActivity(
            "KeyVault Set Secret",
            ActivityKind.Client);

        activity?.SetTag("kv.operation", "set");
        activity?.SetTag("kv.secret.name", name);

        try
        {
            await _client.SetSecretAsync(
                new KeyVaultSecret(name, value),
                cancellationToken);

            _logger.LogInformation(
                "Set Key Vault secret {Name}",
                name);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Failed to set Key Vault secret {Name}",
                name);

            throw;
        }
    }

    public async Task DeleteSecretAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Source.StartActivity(
            "KeyVault Delete Secret",
            ActivityKind.Client);

        activity?.SetTag("kv.operation", "delete");
        activity?.SetTag("kv.secret.name", name);

        try
        {
            await _client.StartDeleteSecretAsync(
                name,
                cancellationToken);

            _logger.LogInformation(
                "Deleted Key Vault secret {Name}",
                name);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Failed to delete Key Vault secret {Name}",
                name);

            throw;
        }
    }
}
