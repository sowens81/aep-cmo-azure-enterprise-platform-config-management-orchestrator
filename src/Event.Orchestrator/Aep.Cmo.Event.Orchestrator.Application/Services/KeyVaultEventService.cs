using Aep.Cmo.Event.Orchestrator.Application.Interfaces;
using Aep.Cmo.Event.Orchestrator.Domain.Models;
using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;
using Aep.Cmo.Shared.Domain;
using Aep.Cmo.Shared.Domain.Results;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Aep.Cmo.Event.Orchestrator.Application.Services;

public class KeyVaultEventService : IKeyVaultEventService
{
    private static readonly ActivitySource ActivitySource =
    new("ConfigManagement.KeyVault.Event.Orchestrator.Application");
    private readonly ILogger<KeyVaultEventService> _logger;
    private readonly IHubKeyVaultSecretClient _hubKeyVaultSecretClient;
    private readonly IKeyVaultTopicPublisherClient _keyVaultTopicPublisherClient;

    public KeyVaultEventService(
        ILogger<KeyVaultEventService> logger,
        IHubKeyVaultSecretClient hubKeyVaultSecretClient,
        IKeyVaultTopicPublisherClient keyVaultTopicPublisherClient)
    {
        _logger = logger;
        _hubKeyVaultSecretClient = hubKeyVaultSecretClient;
        _keyVaultTopicPublisherClient = keyVaultTopicPublisherClient;
    }

    public async Task<Result> EventKeyVaultAsync(KeyVaultEvent message, string correlationId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

}
