using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;

namespace Aep.Cmo.Event.Orchestrator.Infrastructure.Options;

public class AppConfigServiceBusTopicOptions : IAppConfigServiceBusTopicOptions
{
    public string TopicName { get; init; } = default!;
}
