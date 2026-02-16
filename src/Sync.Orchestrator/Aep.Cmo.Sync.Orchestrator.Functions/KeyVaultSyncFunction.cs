using Azure.Messaging.ServiceBus;
using Aep.Cmo.Shared.Domain.Models;
using Aep.Cmo.Sync.Orchestrator.Application.Interfaces;
using Aep.Cmo.Sync.Orchestrator.Functions.Extensions;
using Aep.Cmo.Sync.Orchestrator.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Aep.Cmo.Sync.Orchestrator.Functions;

public class KeyVaultSyncFunction
{
    private readonly ILogger<KeyVaultSyncFunction> _logger;
    private readonly IKeyVaultSyncService _keyVaultSyncService;

    private static readonly ActivitySource ActivitySource =
        new("ConfigManagement.KeyVault.Sync.Function");

    public KeyVaultSyncFunction(ILogger<KeyVaultSyncFunction> logger, IKeyVaultSyncService keyVaultSyncService)
    {
        _logger = logger;
        _keyVaultSyncService = keyVaultSyncService;
    }

    [Function(nameof(KeyVaultSyncFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger(
            "%SBUS_KEY_VAULT_TOPIC%",
            "%SBUS_KEY_VAULT_SUBSCRIPTION%",
            Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken ct)
    {
        SyncMessage<KeyVaultMessage> eventMessage;

        try
        {
            eventMessage = message.Body
                .ToObjectFromJson<SyncMessage<KeyVaultMessage>>()!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize message {MessageId}", message.MessageId);

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
                "Validation failed for message {MessageId}: {Errors}",
                message.MessageId,
                string.Join(" | ", errors));

            await messageActions.DeadLetterWithReasonAsync(
                message,
                "ValidationFailed",
                string.Join(" | ", errors),
                ct);

            return;
        }

        using var activity = ActivitySource.StartActivity(
            "KeyVaultSync.ProcessMessage",
            ActivityKind.Consumer);

        activity?.SetTag("event.id", eventMessage.EventGridId);
        activity?.SetTag("correlation.id", eventMessage.CorrelationId);
        activity?.SetTag("sync.action", eventMessage.Payload.SyncAction.ToString());

        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = eventMessage.CorrelationId,
            ["SyncAction"] = eventMessage.Payload.SyncAction
        }))
        {
            _logger.LogInformation("Starting sync message processing");

            var result = await _keyVaultSyncService
                .SyncKeyVaultAsync(eventMessage.Payload, ct);

            if (!result.IsSuccess)
            {
                activity?.SetStatus(ActivityStatusCode.Error, result.Error);

                _logger.LogWarning(
                    "Non-recoverable sync failure for secret {Secret}: {Error}",
                    eventMessage.Payload.SecretName,
                    result.Error);

                await messageActions.DeadLetterWithReasonAsync(
                    message,
                    "BusinessValidationFailed",
                    result.Error ?? "Unknown error",
                    ct);

                return;
            }

            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation("Sync message processing completed");

            await messageActions.CompleteMessageAsync(message, ct);
        }
    }
}