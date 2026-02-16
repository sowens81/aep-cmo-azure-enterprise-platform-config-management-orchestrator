using Aep.Cmo.Event.Orchestrator.Domain.Models;
using Aep.Cmo.Shared.Domain;
using Aep.Cmo.Shared.Domain.Models;
using Aep.Cmo.Shared.Domain.Results;

namespace Aep.Cmo.Event.Orchestrator.Application.Interfaces;

public interface IKeyVaultEventService
{
    Task<Result<Unit>> EventKeyVaultAsync(EventGridEvent<KeyVaultSecretEventData> message, CancellationToken cancellationToken);
}
    