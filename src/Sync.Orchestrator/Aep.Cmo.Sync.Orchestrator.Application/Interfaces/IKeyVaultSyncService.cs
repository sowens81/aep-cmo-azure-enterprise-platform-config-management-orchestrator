using Aep.Cmo.Shared.Domain;
using Aep.Cmo.Shared.Domain.Models;
using Aep.Cmo.Shared.Domain.Results;

namespace Aep.Cmo.Sync.Orchestrator.Application.Interfaces;

public interface IKeyVaultSyncService
{
    Task<Result<Unit>> SyncKeyVaultAsync(
        KeyVaultMessage message,
        CancellationToken cancellationToken);
}
    