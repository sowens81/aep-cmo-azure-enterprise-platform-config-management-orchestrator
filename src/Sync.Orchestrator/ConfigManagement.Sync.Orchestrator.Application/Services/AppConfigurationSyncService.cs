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
}
