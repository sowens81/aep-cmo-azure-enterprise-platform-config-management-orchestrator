using Azure.Core;

namespace ConfigManagement.Shared.KeyVault.Interfaces;

public interface IKeyVaultCredentialFactory
{
    TokenCredential CreateCredential();
}
