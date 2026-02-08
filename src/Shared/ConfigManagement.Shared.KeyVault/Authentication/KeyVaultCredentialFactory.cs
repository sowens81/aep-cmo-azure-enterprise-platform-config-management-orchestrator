using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Shared.KeyVault.Authentication;

public sealed class KeyVaultCredentialFactory : IKeyVaultCredentialFactory
{
    private readonly KeyVaultAuthOptions _options;
    private readonly ILogger<KeyVaultCredentialFactory> _logger;

    public KeyVaultCredentialFactory(
        KeyVaultAuthOptions options,
        ILogger<KeyVaultCredentialFactory> logger)
    {
        _options = options;
        _logger = logger;
    }

    public TokenCredential CreateCredential()
    {
        _logger.LogInformation(
            "Creating Key Vault credential using {AuthType}",
            _options.AuthType);

        return _options.AuthType switch
        {
            KeyVaultAuthType.Default => new DefaultAzureCredential(),

            KeyVaultAuthType.ManagedIdentity =>
                string.IsNullOrWhiteSpace(_options.ManagedIdentityClientId)
                    ? new ManagedIdentityCredential()
                    : new ManagedIdentityCredential(_options.ManagedIdentityClientId),

            KeyVaultAuthType.ClientSecret =>
                new ClientSecretCredential(
                    _options.TenantId!,
                    _options.ClientId!,
                    _options.ClientSecret!),

            KeyVaultAuthType.AzureCli => new AzureCliCredential(),

            KeyVaultAuthType.VisualStudio => new VisualStudioCredential(),

            _ => throw new NotSupportedException()
        };
    }
}
