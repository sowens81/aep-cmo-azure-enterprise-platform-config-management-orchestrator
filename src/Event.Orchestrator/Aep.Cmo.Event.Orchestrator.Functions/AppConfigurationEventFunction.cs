using Azure.Messaging.ServiceBus;
using Aep.Cmo.Event.Orchestrator.Application.Interfaces;
using Aep.Cmo.Event.Orchestrator.Domain.Models;
using Aep.Cmo.Event.Orchestrator.Functions.Extensions;
using Aep.Cmo.Event.Orchestrator.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Aep.Cmo.Event.Orchestrator.Functions;

public class AppConfigurationEventFunction
{
    private readonly ILogger<AppConfigurationEventFunction> _logger;
    private readonly IAppConfigurationEventService _appConfigurationEventService;

    private static readonly ActivitySource ActivitySource =
        new("ConfigManagement.AppConfiguration.Event.Function");

    public AppConfigurationEventFunction(
        ILogger<AppConfigurationEventFunction> logger,
        IAppConfigurationEventService appConfigurationEventService)
    {
        _logger = logger;
        _appConfigurationEventService = appConfigurationEventService;
    }

    [Function(nameof(AppConfigurationEventFunction))]
    public async Task Run(
    [ServiceBusTrigger(
        "%SBUS_APP_CONFIG_TOPIC%",
        "%SBUS_APP_CONFIG_SUBSCRIPTION%",
        Connection = "ServiceBusConnection"
    )]
    ServiceBusReceivedMessage message,
    ServiceBusMessageActions messageActions,
    CancellationToken ct)
    {
        var correlationId = message.CorrelationId ?? Guid.NewGuid().ToString();

        AppConfigurationEvent eventMessage;

        try
        {
            eventMessage = message.Body
                .ToObjectFromJson<AppConfigurationEvent>()!;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to deserialize message {MessageId}, CorrelationId: {CorrelationId}",
                message.MessageId,
                correlationId);

            await messageActions.DeadLetterWithReasonAsync(
                message,
                "InvalidJson",
                ex.Message,
                ct);

            return;
        }

        if (!ServiceBusMessageValidator.TryValidate(eventMessage, out var errors))
        {
            _logger.LogWarning(
                "Validation failed for message {MessageId}, CorrelationId: {CorrelationId}: {Errors}",
                message.MessageId,
                correlationId,
                string.Join(" | ", errors));

            await messageActions.DeadLetterWithReasonAsync(
                message,
                "ValidationFailed",
                string.Join(" | ", errors),
                ct);

            return;
        }

        using var activity = ActivitySource.StartActivity(
            "AppConfigurationEvent.ProcessMessage",
            ActivityKind.Consumer);

        activity?.SetTag("event.id", eventMessage.Id);
        activity?.SetTag("correlation.id", correlationId);
        activity?.SetTag("message.type", eventMessage.EventType.ToString());

        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["MessageType"] = eventMessage.EventType
        }))
        {
            _logger.LogInformation(
                "Processing AppConfiguration event {EventType} for key {Key}",
                eventMessage.EventType,
                eventMessage.Data.Key);

            var result = await _appConfigurationEventService
                .EventAppConfigurationAsync(eventMessage, ct);

            if (!result.IsSuccess)
            {
                activity?.SetStatus(ActivityStatusCode.Error, result.Error);

                _logger.LogWarning(
                    "Non-recoverable event failure for key {Key}: {Error}",
                    eventMessage.Data.Key,
                    result.Error);

                await messageActions.DeadLetterWithReasonAsync(
                    message,
                    "BusinessValidationFailed",
                    result.Error ?? "Unknown error",
                    ct);

                return;
            }

            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation("Event message processing completed");

            await messageActions.CompleteMessageAsync(message, ct);
        }
    }

}