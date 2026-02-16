using Aep.Cmo.Shared.AppConfiguration;
using Aep.Cmo.Shared.AppConfiguration.Interfaces;
using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aep.Cmo.Event.Orchestrator.Infrastructure.AppConfiguration;

public class HubAppConfigurationClient : AppConfigurationClient, IHubAppConfigurationClient
{
    public HubAppConfigurationClient(
        IHubAppConfigurationOptions options,
        IAppConfigurationCredentialFactory credentialFactory,
        ILogger<HubAppConfigurationClient> logger
        ) : base(options, credentialFactory, logger)
    {
    }
}