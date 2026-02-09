using ConfigManagement.Shared.KeyVault;
using ConfigManagement.Shared.KeyVault.Authentication;
using ConfigManagement.Shared.KeyVault.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigManagement.Sync.Orchestrator.Infrastructure.KeyVault;

public interface ILocalKeyVaultSecretClient : IKeyVaultSecretClient { }

public class LocalKeyVaultSecretClient : KeyVaultSecretClient, ILocalKeyVaultSecretClient
{
    public LocalKeyVaultSecretClient(
        IOptions<LocalKeyVaultOptions> options,
        IKeyVaultCredentialFactory credentialFactory,
        ILogger<KeyVaultSecretClient> logger
        ) : base(options.Value, credentialFactory, logger)
    {

    } 
}
