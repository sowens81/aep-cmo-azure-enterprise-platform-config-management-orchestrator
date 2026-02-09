namespace ConfigManagement.Sync.Orchestrator.Application.Options;

public sealed class ServiceMetaDataOptions
{
    public string Organisation { get; init; } = default!;
    public string Region { get; init; } = default!;
    public string EnvironmentTier { get; init; } = default!;
    public string EnvironmentName { get; init; } = default!;
    public string ServiceName { get; init; } = default!;
}

