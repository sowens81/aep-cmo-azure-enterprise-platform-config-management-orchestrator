using Aep.Cmo.Sync.Orchestrator.Functions.Interfaces;
using Aep.Cmo.Sync.Orchestrator.Functions.Options;

namespace Aep.Cmo.Sync.Orchestrator.Functions.Context;

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
            ["deployment.environment"] = EnvironmentName,
            ["cloud.region"] = Region,
            ["organisation"] = Organisation,
            ["environment.tier"] = EnvironmentTier
        };
}
