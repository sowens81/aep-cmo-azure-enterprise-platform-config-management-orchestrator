using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;

namespace Aep.Cmo.Sync.Orchestrator.Functions.Extensions;

public static class ServiceBusMessageActionsExtensions
{
    public static Task DeadLetterWithReasonAsync(
        this ServiceBusMessageActions actions,
        ServiceBusReceivedMessage message,
        string reason,
        string description,
        CancellationToken token)
    {
        return actions.DeadLetterMessageAsync(
            message,
            deadLetterReason: reason,
            deadLetterErrorDescription: description,
            cancellationToken: token);
    }
}