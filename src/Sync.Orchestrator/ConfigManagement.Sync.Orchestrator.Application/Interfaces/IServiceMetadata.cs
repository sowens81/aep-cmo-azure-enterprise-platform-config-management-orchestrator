namespace ConfigManagement.Sync.Orchestrator.Application.Interfaces;

public interface IServiceMetadata
{
    string Organisation { get; }
    string Region { get; }
    string EnvironmentTier { get; }
    string ServiceName { get; }
}
