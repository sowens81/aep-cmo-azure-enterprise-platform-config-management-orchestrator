using Azure.Core;

namespace ConfigManagement.Shared.ServiceBus.Interfaces;

public interface IServiceBusCredentialFactory
{
    TokenCredential CreateCredential();
}
