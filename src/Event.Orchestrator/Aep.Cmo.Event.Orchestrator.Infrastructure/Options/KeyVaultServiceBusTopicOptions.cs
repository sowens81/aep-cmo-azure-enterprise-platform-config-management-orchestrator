using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;

namespace Aep.Cmo.Event.Orchestrator.Infrastructure.Options;
public class KeyVaultServiceBusTopicOptions : IKeyVaultServiceBusTopicOptions
{
    public string TopicName { get; init; } = default!;
}
