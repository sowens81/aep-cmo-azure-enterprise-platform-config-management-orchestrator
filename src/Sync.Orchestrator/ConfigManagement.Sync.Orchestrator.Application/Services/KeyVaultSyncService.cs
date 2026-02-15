using ConfigManagement.Shared.Domain;
using ConfigManagement.Shared.Domain.Models;
using ConfigManagement.Shared.Domain.Results;
using ConfigManagement.Shared.KeyVault.Interfaces;
using ConfigManagement.Sync.Orchestrator.Application.Interfaces;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Sync.Orchestrator.Application.Services;

public class KeyVaultSyncService : IKeyVaultSyncService
{
    private readonly ILogger<KeyVaultSyncService> _logger;
    private readonly IHubKeyVaultSecretClient _hubKeyVault;
    private readonly IKeyVaultSecretClient _keyVault;

    public KeyVaultSyncService(
        ILogger<KeyVaultSyncService> logger,
        IHubKeyVaultSecretClient hubKeyVault,
        IKeyVaultSecretClient keyVault)
    {
        _logger = logger;
        _hubKeyVault = hubKeyVault;
        _keyVault = keyVault;
    }

    public Task<Result<Unit>> SyncKeyVaultAsync(
        KeyVaultMessage message,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
    