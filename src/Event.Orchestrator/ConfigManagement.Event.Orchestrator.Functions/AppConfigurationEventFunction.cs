using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Event.Orchestrator.Functions;

public class AppConfigurationEventFunction
{
    private readonly ILogger<AppConfigurationEventFunction> _logger;

    public AppConfigurationEventFunction(ILogger<AppConfigurationEventFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(AppConfigurationEventFunction))]
    public async Task Run(
        [ServiceBusTrigger("mytopic", "mysubscription", Connection = "changme")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("MessageID: {id}", message.MessageId);

        await messageActions.CompleteMessageAsync(message);
    }
}