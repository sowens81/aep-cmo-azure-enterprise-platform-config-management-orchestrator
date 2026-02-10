using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;

namespace ConfigManagement.Sync.Orchestrator.Infrastructure.Options;

public class ResultServiceBusTopicOptions : IResultServiceBusTopicOptions
{
    public string TopicName { get; init; } = default!;
}
