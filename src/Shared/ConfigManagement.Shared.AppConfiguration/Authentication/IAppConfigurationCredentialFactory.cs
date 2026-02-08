using Azure.Core;

namespace ConfigManagement.Shared.AppConfiguration.Authentication;

public interface IAppConfigurationCredentialFactory
{
    TokenCredential CreateCredential();
}
