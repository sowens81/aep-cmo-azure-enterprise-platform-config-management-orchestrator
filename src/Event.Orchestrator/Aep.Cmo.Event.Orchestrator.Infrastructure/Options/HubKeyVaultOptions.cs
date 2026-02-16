using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;

namespace Aep.Cmo.Event.Orchestrator.Infrastructure.Options;

public sealed class HubKeyVaultOptions : IHubKeyVaultOptions
{
    public string Endpoint { get; init; } = default!;
}