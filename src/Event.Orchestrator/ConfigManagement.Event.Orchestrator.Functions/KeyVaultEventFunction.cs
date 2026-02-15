using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Event.Orchestrator.Functions;

public class KeyVaultEventFunction
{
    private readonly ILogger<KeyVaultEventFunction> _logger;

    public KeyVaultEventFunction(ILogger<KeyVaultEventFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(KeyVaultEventFunction))]
    public async Task Run(
        [ServiceBusTrigger(
            "mytopic", 
            "mysubscription", 
            Connection = "tbd"
        )]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("MessageID: {id}", message.MessageId);

            // Complete the message
        await messageActions.CompleteMessageAsync(message);
    }
}