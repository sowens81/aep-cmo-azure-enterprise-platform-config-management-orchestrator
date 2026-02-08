using ConfigManagement.Shared.Domain;
using ConfigManagement.Shared.Domain.Models;
using ConfigManagement.Shared.Domain.Results;

namespace ConfigManagement.Sync.Orchestrator.Application.Interfaces;

public interface IConfigSyncHandler
{
    Task<Result<Unit>> HandleAsync(
        ConfigSyncMessage message,
        CancellationToken cancellationToken);
}
