using ConfigManagement.Shared.Domain;
using ConfigManagement.Shared.Domain.Models;
using ConfigManagement.Shared.Domain.Results;

namespace ConfigManagement.Sync.Orchestrator.Application.Interfaces;

public interface IAppConfigurationSyncService
{
    Task<Result<Unit>> SyncAppConfigurationAsync(
        AppConfigMessage message,
        CancellationToken cancellationToken);
}
