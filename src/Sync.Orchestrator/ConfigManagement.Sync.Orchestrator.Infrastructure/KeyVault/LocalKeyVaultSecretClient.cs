using ConfigManagement.Shared.KeyVault;
using ConfigManagement.Shared.KeyVault.Interfaces;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConfigManagement.Sync.Orchestrator.Infrastructure.KeyVault;

public class LocalKeyVaultSecretClient : KeyVaultSecretClient, ILocalKeyVaultSecretClient
{
    public LocalKeyVaultSecretClient(
        IOptions<ILocalKeyVaultOptions> options,
        IKeyVaultCredentialFactory credentialFactory,
        ILogger<KeyVaultSecretClient> logger
        ) : base(options.Value, credentialFactory, logger)
    {

    } 
}
