using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;

namespace ConfigManagement.Sync.Orchestrator.Infrastructure.Options;

public class HubAppConfigurationOptions : IHubAppConfigurationOptions
{
    public string Endpoint { get; init; } = default!;
}
