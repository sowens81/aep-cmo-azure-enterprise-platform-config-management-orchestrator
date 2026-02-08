using ConfigManagement.Shared.Domain;
using ConfigManagement.Shared.Domain.Models;
using ConfigManagement.Shared.Domain.Results;
using ConfigManagement.Shared.ServiceBus.Models;

namespace ConfigManagement.Sync.Orchestrator.Application.Orchestration;

public interface ISyncResultOrchestrator
{
    Task HandleResultAsync(
        EventMessage<ConfigSyncMessage> sourceMessage,
        Result<Unit> result,
        CancellationToken cancellationToken);
}
