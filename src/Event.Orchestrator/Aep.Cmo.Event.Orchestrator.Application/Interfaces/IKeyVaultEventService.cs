using Aep.Cmo.Event.Orchestrator.Domain.Models;
using Aep.Cmo.Shared.Domain;
using Aep.Cmo.Shared.Domain.Results;

namespace Aep.Cmo.Event.Orchestrator.Application.Interfaces;

public interface IKeyVaultEventService
{
    Task<Result> EventKeyVaultAsync(KeyVaultEvent message, string correlationId, CancellationToken cancellationToken);
}
    