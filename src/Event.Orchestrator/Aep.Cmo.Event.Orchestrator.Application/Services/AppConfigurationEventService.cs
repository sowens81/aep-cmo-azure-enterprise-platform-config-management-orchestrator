using Aep.Cmo.Event.Orchestrator.Application.Interfaces;
using Aep.Cmo.Event.Orchestrator.Domain.Models;
using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;
using Aep.Cmo.Event.Orchestrator.Infrastructure.ServiceBus;
using Aep.Cmo.Shared.Domain;
using Aep.Cmo.Shared.Domain.Models;
using Aep.Cmo.Shared.Domain.Results;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Aep.Cmo.Event.Orchestrator.Application.Services;

public class AppConfigurationEventService : IAppConfigurationEventService
{
    private static readonly ActivitySource ActivitySource =
    new("ConfigManagement.AppConfiguration.Event.Orchestrator.Application");
    private readonly ILogger<AppConfigurationEventService> _logger;
    private readonly IHubAppConfigurationClient _hubAppConfigurationClient;
    private readonly IHubKeyVaultSecretClient _hubKeyVaultSecretClient;
    private readonly IAppConfigTopicPublisherClient<AppConfigMessage> _appConfigTopicPublisherClient;

    public AppConfigurationEventService(
        ILogger<AppConfigurationEventService> logger,
        IHubAppConfigurationClient hubAppConfigurationClient,
        IHubKeyVaultSecretClient hubKeyVaultSecretClient,
        IAppConfigTopicPublisherClient<AppConfigMessage> appConfigTopicPublisherClient)
    {
        _logger = logger;
        _hubAppConfigurationClient = hubAppConfigurationClient;
        _hubKeyVaultSecretClient = hubKeyVaultSecretClient;
        _appConfigTopicPublisherClient = appConfigTopicPublisherClient;
    }

    public async Task<Result<Unit>> EventAppConfigurationAsync(EventGridEvent<AppConfigEventData> message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
