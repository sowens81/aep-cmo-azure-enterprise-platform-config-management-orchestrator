using ConfigManagement.Shared.AppConfiguration;
using ConfigManagement.Shared.AppConfiguration.Interfaces;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Sync.Orchestrator.Infrastructure.AppConfiguration;

public class HubAppConfigurationClient : AppConfigurationClient, IHubAppConfigurationClient
{
    public HubAppConfigurationClient(
        IHubAppConfigurationOptions options,
        IAppConfigurationCredentialFactory credentialFactory,
        ILogger<AppConfigurationClient> logger
        ) : base(options, credentialFactory, logger)
    {
    }
}