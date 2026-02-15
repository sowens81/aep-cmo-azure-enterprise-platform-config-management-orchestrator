using ConfigManagement.Shared.Domain.Models;
using ConfigManagement.Sync.Orchestrator.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ConfigManagement.Sync.Orchestrator.Functions;

public sealed class AppConfigurationSyncFunction
{
    private readonly ILogger<AppConfigurationSyncFunction> _logger;
    private readonly IAppConfigurationSyncService _appSyncService;
    private static readonly ActivitySource ActivitySource =
    new("ConfigManagement.Sync.Orchestrator");

    public AppConfigurationSyncFunction(
        ILogger<AppConfigurationSyncFunction> logger,
        IAppConfigurationSyncService appSyncService
    )
    {
        _logger = logger;
        _appSyncService = appSyncService;
    }

    [Function(nameof(AppConfigurationSyncFunction))]
    public async Task RunAsync(
    [ServiceBusTrigger(
        "%SBUS_APP_CONFIG_TOPIC%",
        "%SBUS_APP_CONFIG_SUBSCRIPTION%",
        Connection = "ServiceBusConnection")]
    EventMessage<AppConfigMessage> message,
    CancellationToken ct)
    {
        using var activity = ActivitySource.StartActivity(
            "AppConfigurationSync.ProcessMessage",
            ActivityKind.Consumer);

        activity?.SetTag("event.id", message.EventGridId);
        activity?.SetTag("correlation.id", message.CorrelationId);
        activity?.SetTag("sync.action", message.Payload.SyncAction);

        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = message.CorrelationId,
            ["SyncAction"] = message.Payload.SyncAction
        }))
        {
            _logger.LogInformation("Starting sync message processing");

            var result = await _appSyncService
                .SyncAppConfigurationAsync(message.Payload, ct);

            if (!result.IsSuccess)
            {
                activity?.SetStatus(ActivityStatusCode.Error, result.Error);
                _logger.LogWarning("Sync message processing failed");
                throw new InvalidOperationException(result.Error);
            }

            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation("Sync message processing completed");
        }
    }
}
