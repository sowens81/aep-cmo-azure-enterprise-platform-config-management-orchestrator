using ConfigManagement.Shared.AppConfiguration.Constants;
using ConfigManagement.Shared.AppConfiguration.Interfaces;
using ConfigManagement.Shared.Domain;
using ConfigManagement.Shared.Domain.Enum;
using ConfigManagement.Shared.Domain.Models;
using ConfigManagement.Shared.Domain.Results;
using ConfigManagement.Shared.KeyVault.Interfaces;
using ConfigManagement.Sync.Orchestrator.Application.Interfaces;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace ConfigManagement.Sync.Orchestrator.Application.Services;

public class AppConfigurationSyncService : IAppConfigurationSyncService
{
    private static readonly ActivitySource ActivitySource =
    new("ConfigManagement.Sync.Orchestrator.Application");
    private readonly ILogger<AppConfigurationSyncService> _logger;
    private readonly IAppConfigurationClient _appConfigurationClient;              // Local
    private readonly IHubAppConfigurationClient _hubAppConfigurationClient;        // Hub
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;                  // Local
    private readonly IHubKeyVaultSecretClient _hubKeyVaultSecretClient;            // Hub

    public AppConfigurationSyncService(
        ILogger<AppConfigurationSyncService> logger,
        IAppConfigurationClient appConfigurationClient,
        IHubAppConfigurationClient hubAppConfigurationClient,
        IKeyVaultSecretClient keyVaultSecretClient,
        IHubKeyVaultSecretClient hubKeyVaultSecretClient)
    {
        _logger = logger;
        _appConfigurationClient = appConfigurationClient;
        _hubAppConfigurationClient = hubAppConfigurationClient;
        _keyVaultSecretClient = keyVaultSecretClient;
        _hubKeyVaultSecretClient = hubKeyVaultSecretClient;
    }

    public async Task<Result<Unit>> SyncAppConfigurationAsync(
        AppConfigMessage message,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(
            "AppConfigurationSyncService.Sync",
            ActivityKind.Internal);

        activity?.SetTag("config.key", message.ConfigKeyName);
        activity?.SetTag("config.id", message.ConfigKeyId);
        activity?.SetTag("sync.type", message.Type.ToString());
        activity?.SetTag("sync.action", message.SyncAction.ToString());

        try
        {
            if (message.Type == ConfigSyncMessageType.Value &&
                message.SyncAction == SyncAction.Upsert)
            {
                return await HandleValueUpsertAsync(message, cancellationToken);
            }

            if (message.Type == ConfigSyncMessageType.Value &&
                message.SyncAction == SyncAction.Delete)
            {
                return await HandleValueDeleteAsync(message, cancellationToken);
            }

            if (message.Type == ConfigSyncMessageType.KeyVaultReference &&
                message.SyncAction == SyncAction.Upsert)
            {
                return await HandleKeyVaultReferenceUpsertAsync(message, cancellationToken);
            }

            if (message.Type == ConfigSyncMessageType.KeyVaultReference &&
                message.SyncAction == SyncAction.Delete)
            {
                return await HandleKeyVaultReferenceDeleteAsync(message, cancellationToken);
            }

            _logger.LogWarning(
                "Unsupported sync combination Type={Type}, Action={Action}",
                message.Type,
                message.SyncAction);

            return Result<Unit>.Failure(
                $"Unsupported combination {message.Type} / {message.SyncAction}");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Unhandled error syncing configuration key {Key}",
                message.ConfigKeyName);

            throw; // Allow Service Bus retry
        }
    }

    // ------------------------------------------------------------
    // VALUE + UPSERT
    // ------------------------------------------------------------

    private async Task<Result<Unit>> HandleValueUpsertAsync(
        AppConfigMessage message,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(
            "AppConfigurationSyncService.Value.Upsert",
            ActivityKind.Internal);

        activity?.SetTag("config.key", message.ConfigKeyName);

        _logger.LogInformation(
            "Starting Value Upsert sync for key {Key}",
            message.ConfigKeyName);

        try
        {
            var hubSetting = await _hubAppConfigurationClient
                .GetConfigurationSettingAsync(
                    message.ConfigKeyName,
                    cancellationToken: cancellationToken);

            if (hubSetting is null)
            {
                _logger.LogWarning(
                    "Hub configuration key {Key} not found",
                    message.ConfigKeyName);

                activity?.SetTag("sync.result", "hub_not_found");

                return Result<Unit>.Failure(
                    $"Hub key {message.ConfigKeyName} not found.");
            }

            var hubValue = hubSetting.Value;
            var hubContentType = hubSetting.ContentType ?? ContentTypes.PlainText;

            var localExists = await _appConfigurationClient
                .CheckConfigurationSettingAsync(
                    message.ConfigKeyName,
                    cancellationToken: cancellationToken);

            if (localExists)
            {
                _logger.LogInformation(
                    "Updating local configuration key {Key}",
                    message.ConfigKeyName);

                await _appConfigurationClient
                    .SetConfigurationSettingAsync(
                        message.ConfigKeyName,
                        hubValue,
                        cancellationToken: cancellationToken);

                activity?.SetTag("sync.operation", "update");
            }
            else
            {
                _logger.LogInformation(
                    "Creating local configuration key {Key}",
                    message.ConfigKeyName);

                var created = await _appConfigurationClient
                    .AddConfigurationSettingAsync(
                        message.ConfigKeyName,
                        hubValue,
                        contentType: hubContentType,
                        cancellationToken: cancellationToken);

                if (!created)
                {
                    _logger.LogInformation(
                        "Local key {Key} already existed during create attempt. Falling back to update.",
                        message.ConfigKeyName);

                    await _appConfigurationClient
                        .SetConfigurationSettingAsync(
                            message.ConfigKeyName,
                            hubValue,
                            cancellationToken: cancellationToken);
                }

                activity?.SetTag("sync.operation", "create");
            }

            activity?.SetTag("sync.result", "success");
            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(
                "Successfully synced Value configuration key {Key}",
                message.ConfigKeyName);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Failed to upsert Value configuration key {Key}",
                message.ConfigKeyName);

            throw;
        }
    }

    // ------------------------------------------------------------
    // VALUE + DELETE
    // ------------------------------------------------------------

    private async Task<Result<Unit>> HandleValueDeleteAsync(
        AppConfigMessage message,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(
            "AppConfigurationSyncService.Value.Delete",
            ActivityKind.Internal);

        activity?.SetTag("config.key", message.ConfigKeyName);

        _logger.LogInformation(
            "Starting Value Delete sync for key {Key}",
            message.ConfigKeyName);

        try
        {
            var localExists = await _appConfigurationClient
                .CheckConfigurationSettingAsync(
                    message.ConfigKeyName,
                    cancellationToken: cancellationToken);

            if (!localExists)
            {
                _logger.LogInformation(
                    "Local configuration key {Key} does not exist. Delete treated as idempotent.",
                    message.ConfigKeyName);

                activity?.SetTag("sync.result", "not_found");
                activity?.SetStatus(ActivityStatusCode.Ok);

                return Result<Unit>.Success(Unit.Value);
            }

            await _appConfigurationClient
                .DeleteConfigurationSettingAsync(
                    message.ConfigKeyName,
                    cancellationToken: cancellationToken);

            activity?.SetTag("sync.result", "deleted");
            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(
                "Successfully deleted local configuration key {Key}",
                message.ConfigKeyName);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Failed to delete local configuration key {Key}",
                message.ConfigKeyName);

            throw; // Infrastructure failure → retry
        }
    }

    //------------------------------------------------------------
    // KEY VAULT SECRETS - UPSERT 
    // ------------------------------------------------------------
    private async Task<Result<Unit>> HandleKeyVaultReferenceUpsertAsync(
        AppConfigMessage message,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(
            "AppConfigurationSyncService.KeyVaultReference.Upsert",
            ActivityKind.Internal);

        activity?.SetTag("config.key", message.ConfigKeyName);

        try
        {
            var hubSetting = await _hubAppConfigurationClient
                .GetConfigurationSettingAsync(
                    message.ConfigKeyName,
                    cancellationToken: cancellationToken);

            if (hubSetting is null)
                return Result<Unit>.Failure($"Hub key {message.ConfigKeyName} not found.");

            if (hubSetting.ContentType != ContentTypes.KeyVaultReference)
                return Result<Unit>.Failure($"Hub key is not a KeyVaultReference.");

            var hubSecretUri = ExtractSecretUri(hubSetting.Value);
            var hubSecretName = hubSecretUri.Segments.Last().Trim('/');

            var hubSecret = await _hubKeyVaultSecretClient
                .GetSecretValueAsync(hubSecretName, cancellationToken);

            if (hubSecret is null || hubSecret.Value.Value is null)
                return Result<Unit>.Failure($"Hub secret {hubSecretName} not found.");

            var secretValue = hubSecret.Value.Value;

            var localSecretExists = await _keyVaultSecretClient
                .SecretExistsAsync(hubSecretName, cancellationToken);

            if (!localSecretExists)
            {
                await _keyVaultSecretClient
                    .CreateSecretAsync(hubSecretName, secretValue, cancellationToken);
            }
            else
            {
                await _keyVaultSecretClient
                    .SetSecretAsync(hubSecretName, secretValue, cancellationToken);
            }

            var localSecret = await _keyVaultSecretClient
                .GetSecretValueAsync(hubSecretName, cancellationToken);

            var localSecretUri = localSecret?.Value.Id
                ?? throw new InvalidOperationException("Local secret URI missing.");

            var localSetting = await _appConfigurationClient
                .GetConfigurationSettingAsync(
                    message.ConfigKeyName,
                    cancellationToken: cancellationToken);

            if (localSetting is null)
            {
                await _appConfigurationClient
                    .AddKeyVaultReferenceConfigurationSettingAsync(
                        message.ConfigKeyName,
                        localSecretUri,
                        cancellationToken: cancellationToken);
            }
            else
            {
                var existingUri = ExtractSecretUri(localSetting.Value);

                if (existingUri != localSecretUri)
                {
                    await _appConfigurationClient
                        .SetConfigurationSettingAsync(
                            message.ConfigKeyName,
                            $@"{{""uri"":""{localSecretUri}""}}",
                            cancellationToken: cancellationToken);
                }
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "KeyVaultReference Upsert failed for {Key}", message.ConfigKeyName);
            throw;
        }
    }

    // ------------------------------------------------------------
    // KEY VAULT REFERENCE + DELETE
    // ------------------------------------------------------------

    private async Task<Result<Unit>> HandleKeyVaultReferenceDeleteAsync(
        AppConfigMessage message,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(
            "AppConfigurationSyncService.KeyVaultReference.Delete",
            ActivityKind.Internal);

        activity?.SetTag("config.key", message.ConfigKeyName);

        _logger.LogInformation(
            "Starting KeyVaultReference Delete for key {Key}",
            message.ConfigKeyName);

        try
        {
            // 1️⃣ Get local AppConfig setting
            var localSetting = await _appConfigurationClient
                .GetConfigurationSettingAsync(
                    message.ConfigKeyName,
                    cancellationToken: cancellationToken);

            if (localSetting is null)
            {
                _logger.LogInformation(
                    "Local AppConfig key {Key} does not exist. Delete treated as idempotent.",
                    message.ConfigKeyName);

                activity?.SetTag("sync.result", "not_found");
                activity?.SetStatus(ActivityStatusCode.Ok);

                return Result<Unit>.Success(Unit.Value);
            }

            // 2️⃣ Extract secret URI
            if (localSetting.ContentType != ContentTypes.KeyVaultReference)
            {
                _logger.LogWarning(
                    "Local key {Key} is not a KeyVaultReference. Deleting key only.",
                    message.ConfigKeyName);

                await _appConfigurationClient
                    .DeleteConfigurationSettingAsync(
                        message.ConfigKeyName,
                        cancellationToken: cancellationToken);

                return Result<Unit>.Success(Unit.Value);
            }

            var secretUri = ExtractSecretUri(localSetting.Value);
            var secretName = secretUri.Segments.Last().Trim('/');

            // 3️⃣ Delete AppConfig key
            await _appConfigurationClient
                .DeleteConfigurationSettingAsync(
                    message.ConfigKeyName,
                    cancellationToken: cancellationToken);

            // 4️⃣ Delete secret from local Key Vault
            var secretExists = await _keyVaultSecretClient
                .SecretExistsAsync(secretName, cancellationToken);

            if (secretExists)
            {
                await _keyVaultSecretClient
                    .DeleteSecretAsync(secretName, cancellationToken);

                _logger.LogInformation(
                    "Deleted local KeyVault secret {Secret}",
                    secretName);
            }
            else
            {
                _logger.LogInformation(
                    "Local KeyVault secret {Secret} does not exist. Delete treated as idempotent.",
                    secretName);
            }

            activity?.SetTag("sync.result", "deleted");
            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(
                "Successfully deleted KeyVaultReference key {Key}",
                message.ConfigKeyName);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex,
                "Failed KeyVaultReference Delete for key {Key}",
                message.ConfigKeyName);

            throw;
        }
    }

    private static Uri ExtractSecretUri(string rawValue)
    {
        var doc = JsonDocument.Parse(rawValue);
        var uri = doc.RootElement.GetProperty("uri").GetString();

        return new Uri(uri!);
    }

}
