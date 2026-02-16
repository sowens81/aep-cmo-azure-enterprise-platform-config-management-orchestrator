using Aep.Cmo.Sync.Orchestrator.Infrastructure.Interfaces;

namespace Aep.Cmo.Sync.Orchestrator.Infrastructure.Options;

public sealed class HubKeyVaultOptions : IHubKeyVaultOptions
{
    public string Endpoint { get; init; } = default!;
}