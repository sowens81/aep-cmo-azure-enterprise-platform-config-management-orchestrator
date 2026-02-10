using ConfigManagement.Shared.ServiceBus.Interfaces;

namespace ConfigManagement.Shared.ServiceBus.Options;

public class ServiceBusOptions : IServiceBusOptions
{
    public string Endpoint { get; init; } = default!;
}
