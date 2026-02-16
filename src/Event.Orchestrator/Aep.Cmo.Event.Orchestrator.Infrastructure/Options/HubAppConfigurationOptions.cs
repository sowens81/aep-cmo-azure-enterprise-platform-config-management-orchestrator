using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;

namespace Aep.Cmo.Event.Orchestrator.Infrastructure.Options;

public class HubAppConfigurationOptions : IHubAppConfigurationOptions
{
    public string Endpoint { get; init; } = default!;
}
