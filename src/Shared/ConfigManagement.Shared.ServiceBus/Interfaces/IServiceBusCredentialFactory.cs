using Azure.Core;
using Azure.Messaging.ServiceBus;

namespace ConfigManagement.Shared.ServiceBus.Interfaces;

public interface IServiceBusCredentialFactory
{
    TokenCredential CreateCredential();
}
