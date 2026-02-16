using Aep.Cmo.Event.Orchestrator.Domain.Models;
using Aep.Cmo.Shared.Domain;
using Aep.Cmo.Shared.Domain.Results;

namespace Aep.Cmo.Event.Orchestrator.Application.Interfaces;

public interface IAppConfigurationEventService
{
    Task<Result<Unit>> EventAppConfigurationAsync(AppConfigurationEvent message, CancellationToken cancellationToken);
}
