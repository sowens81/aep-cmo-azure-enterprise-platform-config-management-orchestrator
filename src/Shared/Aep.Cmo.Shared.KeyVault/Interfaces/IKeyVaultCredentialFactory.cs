using Azure.Core;

namespace Aep.Cmo.Shared.KeyVault.Interfaces;

public interface IKeyVaultCredentialFactory
{
    TokenCredential CreateCredential();
}
