using Azure.Core;

namespace ConfigManagement.Shared.AppConfiguration.Interfaces;

public interface IAppConfigurationCredentialFactory
{
    TokenCredential CreateCredential();
}
