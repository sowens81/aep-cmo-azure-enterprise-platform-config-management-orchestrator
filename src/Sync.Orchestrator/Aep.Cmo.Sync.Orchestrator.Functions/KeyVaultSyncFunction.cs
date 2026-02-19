using Aep.Cmo.Shared.Domain.Results;
using Aep.Cmo.Shared.ServiceBus.Messaging;
using Aep.Cmo.Sync.Orchestrator.Application.Interfaces;
using Aep.Cmo.Sync.Orchestrator.Functions.Extensions;
using Aep.Cmo.Sync.Orchestrator.Infrastructure.Messaging;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Aep.Cmo.Sync.Orchestrator.Functions;

public sealed class KeyVaultSyncFunction
{
    private readonly ILogger<KeyVaultSyncFunction> _logger;
    private readonly IKeyVaultSyncService _keyVaultSyncService;

    private static readonly ActivitySource ActivitySource =
        new("ConfigManagement.KeyVault.Sync.Function");

    public KeyVaultSyncFunction(
        ILogger<KeyVaultSyncFunction> logger,
        IKeyVaultSyncService keyVaultSyncService)
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
        TopicMessage<KeyVaultMessage> eventMessage;

        try
        {
            eventMessage = message.Body
                .ToObjectFromJson<TopicMessage<KeyVaultMessage>>()!;
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

        activity?.SetTag("correlation.id", eventMessage.CorrelationId);
        activity?.SetTag("sync.action", eventMessage.Payload.SyncAction.ToString());

        var result = await _keyVaultSyncService
            .SyncKeyVaultAsync(eventMessage.Payload, ct);

        if (result.IsSuccess)
        {
            activity?.SetStatus(ActivityStatusCode.Ok);
            await messageActions.CompleteMessageAsync(message, ct);
            return;
        }

        activity?.SetStatus(ActivityStatusCode.Error, result.Error);

        if (result.IsRetryable)
        {
            await messageActions.AbandonMessageAsync(message, cancellationToken: ct);
            return;
        }

        if (result.ShouldDeadLetter)
        {
            await messageActions.DeadLetterWithReasonAsync(
                message,
                result.ErrorCode ?? "BusinessFailure",
                result.Error ?? "Unknown error",
                ct);
            return;
        }

        await messageActions.AbandonMessageAsync(message, cancellationToken: ct);
    }
}
