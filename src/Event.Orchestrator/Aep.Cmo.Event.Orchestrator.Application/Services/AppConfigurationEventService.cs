using Aep.Cmo.Event.Orchestrator.Application.Interfaces;
using Aep.Cmo.Event.Orchestrator.Domain.Enum;
using Aep.Cmo.Event.Orchestrator.Domain.Models;
using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;
using Aep.Cmo.Shared.AppConfiguration.Constants;
using Aep.Cmo.Shared.Contracts.Enum;
using Aep.Cmo.Shared.Contracts.Messaging;
using Aep.Cmo.Shared.Domain.Enums;
using Aep.Cmo.Shared.Domain.Results;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Aep.Cmo.Event.Orchestrator.Application.Services;

public class AppConfigurationEventService : IAppConfigurationEventService
{
    private static readonly ActivitySource ActivitySource =
        new("ConfigManagement.AppConfiguration.Event.Orchestrator.Application");

    private readonly ILogger<AppConfigurationEventService> _logger;
    private readonly IHubAppConfigurationClient _hubAppConfigurationClient;
    private readonly IAppConfigTopicPublisherClient _publisher;

    public AppConfigurationEventService(
        ILogger<AppConfigurationEventService> logger,
        IHubAppConfigurationClient hubAppConfigurationClient,
        IHubKeyVaultSecretClient hubKeyVaultSecretClient,
        IAppConfigTopicPublisherClient publisher)
    {
        _logger = logger;
        _hubAppConfigurationClient = hubAppConfigurationClient;
        _publisher = publisher;
    }

    public async Task<Result> EventAppConfigurationAsync(
        AppConfigurationEvent message,
        string correlationId,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(
            "AppConfigurationEventService.Event",
            ActivityKind.Internal);

        activity?.SetTag("event.id", message.Id);
        activity?.SetTag("event.type", message.EventType.ToString());
        activity?.SetTag("config.key", message.Data.Key);

        try
        {
            return message.EventType switch
            {
                AppConfigurationEventType.KeyValueModified =>
                    await HandleKeyValueModifiedAsync(message, correlationId, cancellationToken),

                AppConfigurationEventType.KeyValueDeleted =>
                    await HandleKeyValueDeletedAsync(message, correlationId, cancellationToken),

                _ => HandleUnsupportedEvent(message)
            };
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Unhandled error processing AppConfiguration event for key {Key}",
                message.Data.Key);

            return Result.Failure(
                ResultStatus.TransientFailure,
                "Unhandled exception occurred.");
        }
    }

    private async Task<Result> HandleKeyValueModifiedAsync(
        AppConfigurationEvent message,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var key = message.Data.Key;

        var hubSetting = await _hubAppConfigurationClient
            .GetConfigurationSettingAsync(key, "SYNC_SPOKE", cancellationToken: cancellationToken);

        if (hubSetting is null)
        {
            _logger.LogWarning("Hub key {Key} not found", key);

            return Result.Failure(
                ResultStatus.DeadLetter,
                $"Hub key {key} not found.");
        }

        var messageType = DetermineMessageType(hubSetting);

        return await PublishAsync(
            key,
            SyncAction.Upsert,
            messageType,
            correlationId,
            cancellationToken);
    }

    private Task<Result> HandleKeyValueDeletedAsync(
        AppConfigurationEvent message,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var key = message.Data.Key;

        return PublishAsync(
            key,
            SyncAction.Delete,
            ConfigSyncMessageType.Value,
            correlationId,
            cancellationToken);
    }

    private async Task<Result> PublishAsync(
        string key,
        SyncAction action,
        ConfigSyncMessageType type,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var payload = new AppConfigMessage(key, type, action);

        await _publisher.PublishAsync(
            payload,
            correlationId,
            cancellationToken);

        return Result.Success();
    }

    private static ConfigSyncMessageType DetermineMessageType(
        Azure.Data.AppConfiguration.ConfigurationSetting? hubSetting)
    {
        return hubSetting?.ContentType == ContentTypes.KeyVaultReference
            ? ConfigSyncMessageType.KeyVaultReference
            : ConfigSyncMessageType.Value;
    }

    private Result HandleUnsupportedEvent(AppConfigurationEvent message)
    {
        return Result.Failure(
            ResultStatus.Ignore,
            $"Unsupported event type {message.EventType}");
    }
}
