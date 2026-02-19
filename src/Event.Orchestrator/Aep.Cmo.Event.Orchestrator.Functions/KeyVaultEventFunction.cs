using Aep.Cmo.Event.Orchestrator.Application.Interfaces;
using Aep.Cmo.Event.Orchestrator.Domain.Models;
using Aep.Cmo.Event.Orchestrator.Functions.Extensions;
using Aep.Cmo.Event.Orchestrator.Infrastructure.Messaging;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Aep.Cmo.Event.Orchestrator.Functions;

public class KeyVaultEventFunction
{
    private readonly ILogger<KeyVaultEventFunction> _logger;
    private readonly IKeyVaultEventService _keyVaultEventService;

    private static readonly ActivitySource ActivitySource =
        new("ConfigManagement.KeyVault.Event.Function");

    public KeyVaultEventFunction(
        ILogger<KeyVaultEventFunction> logger,
        IKeyVaultEventService keyVaultEventService)
    {
        _logger = logger;
        _keyVaultEventService = keyVaultEventService;
    }

    [Function(nameof(KeyVaultEventFunction))]
    public async Task Run(
        [ServiceBusTrigger(
            "%SBUS_KEY_VAULT_TOPIC%",
            "%SBUS_KEY_VAULT_SUBSCRIPTION%",
            Connection = "ServiceBusConnection"
        )]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken ct
        )
    {
        var correlationId = message.CorrelationId ?? Guid.NewGuid().ToString();

        KeyVaultEvent eventMessage;

        try
        {
            eventMessage = message.Body
                .ToObjectFromJson<KeyVaultEvent>()!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize message {MessageId}, CorrelationId: {CorrelationId}", message.MessageId, correlationId);

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
        activity?.SetTag("message.type", eventMessage.EventType);

        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["MessageType"] = eventMessage.EventType
        }))
        {
            _logger.LogInformation("Starting event message processing");

            var result = await _keyVaultEventService
                .EventKeyVaultAsync(eventMessage, ct);
            if (!result.IsSuccess)
            {
                activity?.SetStatus(ActivityStatusCode.Error, result.Error);

                _logger.LogWarning(
                    "Non-recoverable event failure for KeyVault Secret {Secret}: {Error}",
                    eventMessage.Data.ObjectName,
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