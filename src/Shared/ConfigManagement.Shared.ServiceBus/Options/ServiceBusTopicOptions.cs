using ConfigManagement.Shared.ServiceBus.Interfaces;

namespace ConfigManagement.Shared.ServiceBus.Options;

public class ServiceBusTopicOptions : IServiceBusTopicOptions
{
    public string TopicName { get; init; } = default!;
}
