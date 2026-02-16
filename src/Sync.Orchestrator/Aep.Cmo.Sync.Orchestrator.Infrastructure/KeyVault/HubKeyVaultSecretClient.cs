using Aep.Cmo.Shared.KeyVault;
using Aep.Cmo.Shared.KeyVault.Interfaces;
using Aep.Cmo.Sync.Orchestrator.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aep.Cmo.Sync.Orchestrator.Infrastructure.KeyVault;

public class HubKeyVaultSecretClient : KeyVaultSecretClient, IHubKeyVaultSecretClient
{
    public HubKeyVaultSecretClient(
        IHubKeyVaultOptions options,
        IKeyVaultCredentialFactory credentialFactory,
        ILogger<HubKeyVaultSecretClient> logger
        ) : base(options, credentialFactory, logger)
    {

    } 
}

