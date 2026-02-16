using Aep.Cmo.Event.Orchestrator.Application.Interfaces;
using Aep.Cmo.Event.Orchestrator.Domain.Enum;
using Aep.Cmo.Event.Orchestrator.Domain.Models;
using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;
using Aep.Cmo.Shared.AppConfiguration.Constants;
using Aep.Cmo.Shared.Domain;
using Aep.Cmo.Shared.Domain.Enum;
using Aep.Cmo.Shared.Domain.Models;
using Aep.Cmo.Shared.Domain.Results;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using System.Diagnostics;

namespace Aep.Cmo.Event.Orchestrator.Application.Services;

public class AppConfigurationEventService : IAppConfigurationEventService
{
    private static readonly ActivitySource ActivitySource =
        new("ConfigManagement.AppConfiguration.Event.Orchestrator.Application");

    private readonly ILogger<AppConfigurationEventService> _logger;
    private readonly IHubAppConfigurationClient _hubAppConfigurationClient;
    private readonly IAppConfigTopicPublisherClient<AppConfigMessage> _publisher;

    public AppConfigurationEventService(
        ILogger<AppConfigurationEventService> logger,
        IHubAppConfigurationClient hubAppConfigurationClient,
        IHubKeyVaultSecretClient hubKeyVaultSecretClient, // kept for DI consistency
        IAppConfigTopicPublisherClient<AppConfigMessage> publisher)
    {
        _logger = logger;
        _hubAppConfigurationClient = hubAppConfigurationClient;
        _publisher = publisher;
    }

    public async Task<Result<Unit>> EventAppConfigurationAsync(
        AppConfigurationEvent message,
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
                    await HandleKeyValueModifiedAsync(message, cancellationToken),

                AppConfigurationEventType.KeyValueDeleted =>
                    await HandleKeyValueDeletedAsync(message, cancellationToken),

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

            throw; // allow ServiceBus retry
        }
    }

    // ------------------------------------------------------------
    // MODIFIED FLOW
    // ------------------------------------------------------------

    private async Task<Result<Unit>> HandleKeyValueModifiedAsync(
        AppConfigurationEvent message,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(
            "AppConfigurationEventService.HandleKeyValueModified",
            ActivityKind.Internal);

        var key = message.Data.Key;

        activity?.SetTag("config.key", key);
        activity?.SetTag("sync.action", SyncAction.Upsert.ToString());

        _logger.LogInformation(
            "Processing KeyValueModified event for key {Key}",
            key);

        var hubSetting = await _hubAppConfigurationClient
            .GetConfigurationSettingAsync(key, cancellationToken: cancellationToken);

        if (hubSetting is null)
        {
            _logger.LogWarning(
                "Hub key {Key} not found for modified event",
                key);

            activity?.SetStatus(ActivityStatusCode.Error, "Hub key not found");

            return Result<Unit>.Failure($"Hub key {key} not found.");
        }

        var messageType = DetermineMessageType(hubSetting);

        return await PublishAsync(
            message,
            key,
            SyncAction.Upsert,
            messageType,
            cancellationToken);
    }

    // ------------------------------------------------------------
    // DELETE FLOW
    // ------------------------------------------------------------

    private async Task<Result<Unit>> HandleKeyValueDeletedAsync(
        AppConfigurationEvent message,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(
            "AppConfigurationEventService.HandleKeyValueDeleted",
            ActivityKind.Internal);

        var key = message.Data.Key;

        activity?.SetTag("config.key", key);
        activity?.SetTag("sync.action", SyncAction.Delete.ToString());

        _logger.LogInformation(
            "Processing KeyValueDeleted event for key {Key}",
            key);

        // Do NOT call App Configuration
        // Sync layer will determine actual type during delete

        return await PublishAsync(
            message,
            key,
            SyncAction.Delete,
            ConfigSyncMessageType.Value, // default
            cancellationToken);
    }

    // ------------------------------------------------------------
    // PUBLISH LOGIC
    // ------------------------------------------------------------

    private async Task<Result<Unit>> PublishAsync(
        AppConfigurationEvent message,
        string key,
        SyncAction action,
        ConfigSyncMessageType type,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(
            "AppConfigurationEventService.Publish",
            ActivityKind.Internal);

        activity?.SetTag("config.key", key);
        activity?.SetTag("sync.action", action.ToString());
        activity?.SetTag("sync.type", type.ToString());

        var correlationId =
            Baggage.GetBaggage("correlation.id")
            ?? Activity.Current?.TraceId.ToString()
            ?? Guid.NewGuid().ToString();

        var payload = new AppConfigMessage
        {
            ConfigKeyName = key,
            SyncAction = action,
            Type = type
        };

        var syncMessage = new SyncMessage<AppConfigMessage>
        {
            CorrelationId = correlationId,
            EventGridId = message.Id,
            Payload = payload
        };

        await _publisher.PublishAsync(syncMessage, cancellationToken);

        activity?.SetStatus(ActivityStatusCode.Ok);

        _logger.LogInformation(
            "Published sync message for key {Key} Action={Action} Type={Type}",
            key,
            action,
            type);

        return Result<Unit>.Success(Unit.Value);
    }

    // ------------------------------------------------------------
    // HELPERS
    // ------------------------------------------------------------

    private static ConfigSyncMessageType DetermineMessageType(
        Azure.Data.AppConfiguration.ConfigurationSetting? hubSetting)
    {
        return hubSetting?.ContentType == ContentTypes.KeyVaultReference
            ? ConfigSyncMessageType.KeyVaultReference
            : ConfigSyncMessageType.Value;
    }

    private Result<Unit> HandleUnsupportedEvent(AppConfigurationEvent message)
    {
        _logger.LogWarning(
            "Unsupported AppConfiguration event type {EventType}",
            message.EventType);

        return Result<Unit>.Failure(
            $"Unsupported event type {message.EventType}");
    }
}
