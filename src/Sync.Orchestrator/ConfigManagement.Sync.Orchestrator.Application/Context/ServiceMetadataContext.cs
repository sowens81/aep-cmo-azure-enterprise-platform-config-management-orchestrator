using ConfigManagement.Sync.Orchestrator.Application.Interfaces;
using ConfigManagement.Sync.Orchestrator.Application.Options;

namespace ConfigManagement.Sync.Orchestrator.Application.Context;

/// <summary>
/// Default implementation of <see cref="IServiceMetadata"/>.
/// </summary>
public sealed class ServiceMetadataContext : IServiceMetadata
{
    public string Organisation { get; }
    public string Region { get; }
    public string EnvironmentTier { get; }
    public string EnvironmentName { get; }
    public string ServiceName { get; }

    public ServiceMetadataContext(ServiceMetaDataOptions options)
    {
        Organisation = options.Organisation;
        Region = options.Region;
        EnvironmentTier = options.EnvironmentTier;
        EnvironmentName = options.EnvironmentName;
        ServiceName = options.ServiceName;
    }

    public IReadOnlyDictionary<string, object> ToResourceAttributes()
        => new Dictionary<string, object>
        {
            ["organisation"] = Organisation,
            ["region"] = Region,
            ["environment.tier"] = EnvironmentTier,
            ["environment.name"] = EnvironmentName
        };
}
