using ConfigManagement.Shared.Domain;
using ConfigManagement.Shared.Domain.Models;
using ConfigManagement.Shared.Domain.Results;

namespace ConfigManagement.Sync.Orchestrator.Application.Interfaces;

public interface IKeyVaultSyncService
{
    Task<Result<Unit>> SyncKeyVaultAsync(
        KeyVaultMessage message,
        CancellationToken cancellationToken);
}
    