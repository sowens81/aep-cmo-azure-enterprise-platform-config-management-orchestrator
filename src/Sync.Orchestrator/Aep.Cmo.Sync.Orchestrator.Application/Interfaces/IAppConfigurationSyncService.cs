using Aep.Cmo.Shared.Domain;
using Aep.Cmo.Shared.Domain.Models;
using Aep.Cmo.Shared.Domain.Results;

namespace Aep.Cmo.Sync.Orchestrator.Application.Interfaces;

public interface IAppConfigurationSyncService
{
    Task<Result<Unit>> SyncAppConfigurationAsync(
        AppConfigMessage message,
        CancellationToken cancellationToken);
}
