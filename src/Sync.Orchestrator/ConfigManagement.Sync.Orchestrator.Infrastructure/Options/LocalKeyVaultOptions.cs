using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;

namespace ConfigManagement.Sync.Orchestrator.Infrastructure.Options;

public sealed class LocalKeyVaultOptions : ILocalKeyVaultOptions
{
    public string Endpoint { get; init; } = default!;
}