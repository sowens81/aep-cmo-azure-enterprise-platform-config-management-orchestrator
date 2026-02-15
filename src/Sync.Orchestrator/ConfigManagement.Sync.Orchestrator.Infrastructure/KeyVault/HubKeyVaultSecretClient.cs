using ConfigManagement.Shared.KeyVault;
using ConfigManagement.Shared.KeyVault.Interfaces;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Sync.Orchestrator.Infrastructure.KeyVault;

public class HubKeyVaultSecretClient : KeyVaultSecretClient, IHubKeyVaultSecretClient
{
    public HubKeyVaultSecretClient(
        IHubKeyVaultOptions options,
        IKeyVaultCredentialFactory credentialFactory,
        ILogger<KeyVaultSecretClient> logger
        ) : base(options, credentialFactory, logger)
    {

    } 
}

