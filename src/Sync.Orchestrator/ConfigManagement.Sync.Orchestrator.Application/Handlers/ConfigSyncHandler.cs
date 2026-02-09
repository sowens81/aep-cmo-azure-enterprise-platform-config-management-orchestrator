using ConfigManagement.Shared.AppConfiguration.Interfaces;
using ConfigManagement.Shared.Domain;
using ConfigManagement.Shared.Domain.Enum;
using ConfigManagement.Shared.Domain.Models;
using ConfigManagement.Shared.Domain.Results;
using ConfigManagement.Shared.KeyVault;
using ConfigManagement.Sync.Orchestrator.Application.Interfaces;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Sync.Orchestrator.Application;

public sealed class ConfigSyncHandler : IConfigSyncHandler
{
    
    private readonly IHubKeyVaultSecretClient _hubKeyVault;
    private readonly ILocalKeyVaultSecretClient _localKeyVault;
    private readonly IAppConfigurationClient _appConfig;
    private readonly ILogger<ConfigSyncHandler> _logger;

    public ConfigSyncHandler(
        IHubKeyVaultSecretClient hubKeyVault,
        ILocalKeyVaultSecretClient localKeyVault,
        IAppConfigurationClient appConfig,
        ILogger<ConfigSyncHandler> logger)
    {
        _hubKeyVault = hubKeyVault;
        _localKeyVault = localKeyVault;
        _appConfig = appConfig;
        _logger = logger;
    }

    public async Task<Result<Unit>> HandleAsync(
        ConfigSyncMessage message,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting config sync. Key={Key}, Type={Type}, Action={Action}",
            message.Key,
            message.Type,
            message.SyncAction);

        try
        {
            return message.Type switch
            {
                ConfigSyncMessageType.Value =>
                    await HandleValueAsync(message, cancellationToken),

                ConfigSyncMessageType.KeyVaultReference =>
                    await HandleKeyVaultReferenceAsync(message, cancellationToken),

                _ => Result<Unit>.Failure(
                    $"Unsupported ConfigSyncMessageType: {message.Type}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception during config sync. Key={Key}",
                message.Key);

            return Result<Unit>.Failure(ex.Message);
        }
    }

    // ---------------------------------------------------------------------
    // VALUE SYNC
    // ---------------------------------------------------------------------

    private async Task<Result<Unit>> HandleValueAsync(
        ConfigSyncMessage message,
        CancellationToken ct)
    {
        if (message.Value is null)
        {
            _logger.LogWarning(
                "Value sync requested but Value is null. Key={Key}",
                message.Key);

            return Result<Unit>.Failure("Value is required for Value sync.");
        }

        if (message.SyncAction == SyncAction.Upsert)
        {
            _logger.LogInformation(
                "Upserting App Configuration value. Key={Key}",
                message.Key);

            await _appConfig.SetConfigurationSettingAsync(
                message.Key,
                message.Value,
                cancellationToken: ct);

            return Result<Unit>.Success(Unit.Value);
        }

        // DELETE
        var exists = await _appConfig.GetConfigurationSettingAsync(
            message.Key,
            cancellationToken: ct);

        if (!exists)
        {
            _logger.LogWarning(
                "Delete requested but App Configuration key does not exist. Key={Key}",
                message.Key);

            return Result<Unit>.Failure("Key does not exist for delete.");
        }

        _logger.LogInformation(
            "Deleting App Configuration value. Key={Key}",
            message.Key);

        await _appConfig.DeleteConfigurationSettingAsync(
            message.Key,
            cancellationToken: ct);

        return Result<Unit>.Success(Unit.Value);
    }

    // ---------------------------------------------------------------------
    // KEY VAULT REFERENCE SYNC
    // ---------------------------------------------------------------------

    // Plan (pseudocode):
    // 1. Validate KeyVaultSecretUri is present.
    // 2. Build hub secret URI and extract secret name.
    // 3. Log processing details.
    // 4. Check if App Configuration key exists.
    // 5. If Upsert:
    //    a. Retrieve secret from hub Key Vault.
    //    b. If the retrieved secret or its Value is null -> log error and return failure.
    //    c. Copy secret value to local Key Vault (create or update depending on existence).
    //    d. If App Configuration key doesn't exist, create a Key Vault reference setting.
    // 6. If Delete:
    //    a. If App Configuration key doesn't exist -> warn and return failure.
    //    b. Delete local Key Vault secret and App Configuration key.
    // Note: Use safe null checks (null-conditional) so we don't access members on a null object.

    private async Task<Result<Unit>> HandleKeyVaultReferenceAsync(
        ConfigSyncMessage message,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(message.KeyVaultSecretUri))
        {
            _logger.LogWarning(
                "KeyVaultReference sync requested but KeyVaultSecretUri is missing. Key={Key}",
                message.Key);

            return Result<Unit>.Failure("KeyVaultSecretUri is required.");
        }

        var hubSecretUri = new Uri(message.KeyVaultSecretUri);
        var secretName = GetSecretNameFromUri(hubSecretUri);

        _logger.LogInformation(
            "Processing Key Vault reference sync. Key={Key}, SecretName={SecretName}",
            message.Key,
            secretName);

        var keyExists = await _appConfig.GetConfigurationSettingAsync(
            message.Key,
            cancellationToken: ct);

        if (message.SyncAction == SyncAction.Upsert)
        {
            var hubSecret = await _hubKeyVault.GetSecretValueAsync(
                secretName,
                ct);

            // Safe null check: hubSecret?.Value will be null if hubSecret is null OR hubSecret.Value is null
            if (hubSecret?.Value == null)
            {
                _logger.LogError(
                    "Hub Key Vault secret not found or empty. Secret={SecretName}",
                    secretName);

                return Result<Unit>.Failure("Hub Key Vault secret not found or empty.");
            }

            var secretValue = hubSecret.Value.Value;

            if (await _localKeyVault.SecretExistsAsync(secretName, ct))
            {
                _logger.LogInformation(
                    "Updating local Key Vault secret. Secret={SecretName}",
                    secretName);

                await _localKeyVault.SetSecretAsync(
                    secretName,
                    secretValue,
                    ct);
            }
            else
            {
                _logger.LogInformation(
                    "Creating local Key Vault secret. Secret={SecretName}",
                    secretName);

                await _localKeyVault.CreateSecretAsync(
                    secretName,
                    secretValue,
                    ct);
            }

            if (!keyExists)
            {
                _logger.LogInformation(
                    "Creating App Configuration Key Vault reference. Key={Key}",
                    message.Key);

                await _appConfig.AddKeyVaultReferenceConfigurationSettingAsync(
                    message.Key,
                    hubSecretUri,
                    cancellationToken: ct);
            }

            return Result<Unit>.Success(Unit.Value);
        }

        // DELETE
        if (!keyExists)
        {
            _logger.LogWarning(
                "Delete requested but App Configuration key does not exist. Key={Key}",
                message.Key);

            return Result<Unit>.Failure("Key does not exist for delete.");
        }

        _logger.LogInformation(
            "Deleting local Key Vault secret and App Configuration key. Key={Key}, Secret={SecretName}",
            message.Key,
            secretName);

        await _localKeyVault.DeleteSecretAsync(secretName, ct);

        await _appConfig.DeleteConfigurationSettingAsync(
            message.Key,
            cancellationToken: ct);

        return Result<Unit>.Success(Unit.Value);
    }

    // ---------------------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------------------

    private static string GetSecretNameFromUri(Uri secretUri)
    {
        var segments = secretUri.AbsolutePath.Split(
            '/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 2 ||
            !segments[0].Equals("secrets", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Invalid Key Vault secret URI: {secretUri}");
        }

        return segments[1];
    }
}
