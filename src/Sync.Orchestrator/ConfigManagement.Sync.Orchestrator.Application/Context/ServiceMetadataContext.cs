using ConfigManagement.Sync.Orchestrator.Application.Interfaces;
using ConfigManagement.Sync.Orchestrator.Application.Options;

namespace ConfigManagement.Sync.Orchestrator.Application.Context;

public sealed class ServiceMetadataContext : IServiceMetadata
{
    public ServiceMetadataContext(ServiceMetaDataOptions options)
    {
        Organisation = options.Organisation;
        Region = options.Region;
        EnvironmentTier = options.EnvironmentTier;
        EnvironmentName = options.EnvironmentName;
        ServiceName = options.ServiceName;
    }

    public string Organisation { get; }
    public string Region { get; }
    public string EnvironmentTier { get; }
    public string EnvironmentName { get; }
    public string ServiceName { get; }

}
