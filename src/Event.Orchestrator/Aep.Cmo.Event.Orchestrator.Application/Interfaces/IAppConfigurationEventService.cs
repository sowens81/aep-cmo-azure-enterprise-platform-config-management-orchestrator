using Aep.Cmo.Event.Orchestrator.Domain.Models;
using Aep.Cmo.Shared.Domain.Results;

namespace Aep.Cmo.Event.Orchestrator.Application.Interfaces;

public interface IAppConfigurationEventService
{
    Task<Result> EventAppConfigurationAsync(
        AppConfigurationEvent message,
        string correlationId,
        CancellationToken cancellationToken);
}
