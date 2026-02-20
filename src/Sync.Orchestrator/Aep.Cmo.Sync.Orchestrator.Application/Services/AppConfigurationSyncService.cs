using Aep.Cmo.Shared.AppConfiguration.Constants;
using Aep.Cmo.Shared.AppConfiguration.Interfaces;
using Aep.Cmo.Shared.Contracts.Enum;
using Aep.Cmo.Shared.Contracts.Messaging;
using Aep.Cmo.Shared.Domain.Enums;
using Aep.Cmo.Shared.Domain.Results;
using Aep.Cmo.Shared.KeyVault.Interfaces;
using Aep.Cmo.Sync.Orchestrator.Application.Interfaces;
using Aep.Cmo.Sync.Orchestrator.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace Aep.Cmo.Sync.Orchestrator.Application.Services;

public class AppConfigurationSyncService : IAppConfigurationSyncService
{
    private static readonly ActivitySource ActivitySource =
        new("ConfigManagement.AppConfiguration.Sync.Orchestrator.Application");

    private readonly ILogger<AppConfigurationSyncService> _logger;
    private readonly IAppConfigurationClient _appConfigurationClient;
    private readonly IHubAppConfigurationClient _hubAppConfigurationClient;
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;
    private readonly IHubKeyVaultSecretClient _hubKeyVaultSecretClient;

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

    public async Task<Result> SyncAppConfigurationAsync(
        AppConfigMessage message,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(
            "AppConfigurationSyncService.Sync",
            ActivityKind.Internal);

        activity?.SetTag("config.key", message.ConfigKeyName);
        activity?.SetTag("sync.type", message.Type.ToString());
        activity?.SetTag("sync.action", message.SyncAction.ToString());

        try
        {
            return (message.Type, message.SyncAction) switch
            {
                (ConfigSyncMessageType.Value, SyncAction.Upsert) =>
                    await HandleValueUpsertAsync(message, cancellationToken),

                (ConfigSyncMessageType.Value, SyncAction.Delete) =>
                    await HandleValueDeleteAsync(message, cancellationToken),

                (ConfigSyncMessageType.KeyVaultReference, SyncAction.Upsert) =>
                    await HandleKeyVaultReferenceUpsertAsync(message, cancellationToken),

                (ConfigSyncMessageType.KeyVaultReference, SyncAction.Delete) =>
                    await HandleKeyVaultReferenceDeleteAsync(message, cancellationToken),

                _ => Result.Failure(
                        ResultStatus.Ignore,
                        $"Unsupported combination {message.Type} / {message.SyncAction}")
            };
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Unhandled error syncing configuration key {Key}",
                message.ConfigKeyName);

            return Result.Failure(
                ResultStatus.TransientFailure,
                "Unhandled infrastructure failure.");
        }
    }

    private async Task<Result> HandleValueUpsertAsync(
        AppConfigMessage message,
        CancellationToken cancellationToken)
    {
        var hubSetting = await _hubAppConfigurationClient
            .GetConfigurationSettingAsync(
                message.ConfigKeyName,
                message.Label,
                cancellationToken: cancellationToken);

        if (hubSetting is null)
        {
            return Result.Failure(
                ResultStatus.DeadLetter,
                $"Hub key {message.ConfigKeyName} not found.");
        }

        var hubValue = hubSetting.Value;
        var hubContentType = hubSetting.ContentType ?? ContentTypes.PlainText;

        var localExists = await _appConfigurationClient
            .CheckConfigurationSettingAsync(
                message.ConfigKeyName,
                message.Label,
                cancellationToken: cancellationToken);

        if (localExists)
        {
            await _appConfigurationClient
                .SetConfigurationSettingAsync(
                    message.ConfigKeyName,
                    hubValue,
                    message.Label,
                    cancellationToken: cancellationToken);
        }
        else
        {
            await _appConfigurationClient
                .AddConfigurationSettingAsync(
                    message.ConfigKeyName,
                    hubValue,
                    message.Label,
                    contentType: hubContentType,
                    cancellationToken: cancellationToken);
        }

        return Result.Success();
    }

    private async Task<Result> HandleValueDeleteAsync(
        AppConfigMessage message,
        CancellationToken cancellationToken)
    {
        var localExists = await _appConfigurationClient
            .CheckConfigurationSettingAsync(
                message.ConfigKeyName,
                message.Label,
                cancellationToken: cancellationToken);

        if (!localExists)
            return Result.Success();

        await _appConfigurationClient
            .DeleteConfigurationSettingAsync(
                message.ConfigKeyName,
                message.Label,
                cancellationToken: cancellationToken);

        return Result.Success();
    }

    private async Task<Result> HandleKeyVaultReferenceUpsertAsync(
    AppConfigMessage message,
    CancellationToken cancellationToken)
    {
        var hubSetting = await _hubAppConfigurationClient
            .GetConfigurationSettingAsync(
                message.ConfigKeyName,
                message.Label,
                cancellationToken: cancellationToken);

        if (hubSetting is null)
            return Result.Failure(
                ResultStatus.DeadLetter,
                $"Hub key {message.ConfigKeyName} not found.");

        if (hubSetting.ContentType != ContentTypes.KeyVaultReference)
            return Result.Failure(
                ResultStatus.ValidationError,
                "Hub key is not a KeyVaultReference.");

        // 1️⃣ Extract hub secret info
        var hubSecretUri = ExtractSecretUri(hubSetting.Value);
        var hubSecretName = hubSecretUri.Segments.Last().Trim('/');

        // 2️⃣ Get hub secret value
        var hubSecret = await _hubKeyVaultSecretClient
            .GetSecretValueAsync(hubSecretName, cancellationToken);

        if (hubSecret is null || hubSecret.Value.Value is null)
            return Result.Failure(
                ResultStatus.DeadLetter,
                $"Hub secret {hubSecretName} not found.");

        var secretValue = hubSecret.Value.Value;

        // 3️⃣ Create or update local secret
        if (!await _keyVaultSecretClient.SecretExistsAsync(hubSecretName, cancellationToken))
            await _keyVaultSecretClient.CreateSecretAsync(hubSecretName, secretValue, cancellationToken);
        else
            await _keyVaultSecretClient.SetSecretAsync(hubSecretName, secretValue, cancellationToken);

        // 4️⃣ Build LOCAL secret URI (this is the important part)
        var localSecretUri = new Uri($"{_keyVaultSecretClient.VaultUri.AbsoluteUri.TrimEnd('/')}/secrets/{hubSecretName}");

        var keyVaultReferenceValue = JsonSerializer.Serialize(new
        {
            uri = localSecretUri.ToString()
        });

        // 5️⃣ Create or update local App Configuration key
        var localExists = await _appConfigurationClient
            .CheckConfigurationSettingAsync(
                message.ConfigKeyName,
                message.Label,
                cancellationToken: cancellationToken);

        if (localExists)
        {
            await _appConfigurationClient
                .SetConfigurationSettingAsync(
                    message.ConfigKeyName,
                    keyVaultReferenceValue,
                    message.Label,
                    cancellationToken: cancellationToken);
        }
        else
        {
            await _appConfigurationClient
                .AddConfigurationSettingAsync(
                    message.ConfigKeyName,
                    keyVaultReferenceValue,
                    message.Label,
                    contentType: ContentTypes.KeyVaultReference,
                    cancellationToken: cancellationToken);
        }

        return Result.Success();
    }

    private async Task<Result> HandleKeyVaultReferenceDeleteAsync(
        AppConfigMessage message,
        CancellationToken cancellationToken)
    {
        await _appConfigurationClient
            .DeleteConfigurationSettingAsync(
                message.ConfigKeyName,
                message.Label,
                cancellationToken: cancellationToken);

        return Result.Success();
    }

    private static Uri ExtractSecretUri(string rawValue)
    {
        var doc = JsonDocument.Parse(rawValue);
        var uri = doc.RootElement.GetProperty("uri").GetString();
        return new Uri(uri!);
    }
}
