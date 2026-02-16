using Azure.Core;

namespace Aep.Cmo.Shared.AppConfiguration.Interfaces;

public interface IAppConfigurationCredentialFactory
{
    TokenCredential CreateCredential();
}
