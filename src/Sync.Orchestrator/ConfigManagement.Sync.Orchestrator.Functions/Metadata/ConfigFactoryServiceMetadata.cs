using ConfigManagement.Sync.Orchestrator.Application.Interfaces;
using ConfigurationSyncOrchestrator.Domain.Factories;

namespace ConfigManagement.Sync.Orchestrator.Functions.Metadata;

public sealed class ConfigFactoryServiceMetadata : IServiceMetadata
{
    private readonly ConfigFactory _config;

    public ConfigFactoryServiceMetadata(ConfigFactory config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public string Organisation => _config.Organisation;
    public string Region => _config.Region;
    public string EnvironmentTier => _config.EnvironmentTier;
    public string ServiceName => _config.ServiceName;
}
