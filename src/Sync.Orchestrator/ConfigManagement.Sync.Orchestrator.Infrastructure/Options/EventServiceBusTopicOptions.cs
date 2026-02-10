using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;

namespace ConfigManagement.Sync.Orchestrator.Infrastructure.Options;

internal class EventServiceBusTopicOptions : IEventServiceBusTopicOptions
{
    public string TopicName { get; init; } = default!;
}
