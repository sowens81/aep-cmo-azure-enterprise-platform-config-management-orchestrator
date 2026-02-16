using Aep.Cmo.Sync.Orchestrator.Infrastructure.Interfaces;

namespace Aep.Cmo.Sync.Orchestrator.Infrastructure.Options;

public class HubAppConfigurationOptions : IHubAppConfigurationOptions
{
    public string Endpoint { get; init; } = default!;
}
