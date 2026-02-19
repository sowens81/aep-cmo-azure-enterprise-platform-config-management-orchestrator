using Aep.Cmo.Shared.Contracts.Messaging;
using Aep.Cmo.Shared.Domain.Results;

namespace Aep.Cmo.Sync.Orchestrator.Application.Interfaces;

public interface IAppConfigurationSyncService
{
    Task<Result> SyncAppConfigurationAsync(
        AppConfigMessage message,
        CancellationToken cancellationToken);
}
