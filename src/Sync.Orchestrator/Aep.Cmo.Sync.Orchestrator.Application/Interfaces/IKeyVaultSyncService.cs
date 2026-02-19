using Aep.Cmo.Shared.Domain.Results;
using Aep.Cmo.Shared.ServiceBus.Messaging;

namespace Aep.Cmo.Sync.Orchestrator.Application.Interfaces;

public interface IKeyVaultSyncService
{
    Task<Result> SyncKeyVaultAsync(
        KeyVaultMessage message,
        CancellationToken cancellationToken);
}
