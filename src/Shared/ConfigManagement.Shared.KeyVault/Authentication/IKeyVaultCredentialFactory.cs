using Azure.Core;

namespace ConfigManagement.Shared.KeyVault.Authentication;

public interface IKeyVaultCredentialFactory
{
    TokenCredential CreateCredential();
}
