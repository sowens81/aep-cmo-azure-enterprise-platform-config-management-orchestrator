using Azure.Messaging.ServiceBus;

namespace ConfigManagement.Shared.ServiceBus.Authentication;

public interface IServiceBusCredentialFactory
{
    ServiceBusClient CreateClient();
}
