using Azure.Core;
using Azure.Identity;
using ConfigManagement.Shared.KeyVault.Enums;
using ConfigManagement.Shared.KeyVault.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Shared.KeyVault.Authentication;

public sealed class KeyVaultCredentialFactory : IKeyVaultCredentialFactory
{
    private readonly IKeyVaultAuthOptions _options;
    private readonly ILogger<KeyVaultCredentialFactory> _logger;

    public KeyVaultCredentialFactory(
        IKeyVaultAuthOptions options,
        ILogger<KeyVaultCredentialFactory> logger)
    {
        _options = options;
        _logger = logger;
    }

    public TokenCredential CreateCredential()
    {
        try
        {
            _logger.LogInformation(
                "Creating Key Vault credential using {AuthType}",
                _options.AuthType);

            return _options.AuthType switch
            {
                AuthType.Default => new DefaultAzureCredential(),

                AuthType.ManagedIdentity =>
                    string.IsNullOrWhiteSpace(_options.ManagedIdentityClientId)
                        ? new ManagedIdentityCredential()
                        : new ManagedIdentityCredential(_options.ManagedIdentityClientId),

                AuthType.ClientSecret =>
                    new ClientSecretCredential(
                        _options.TenantId!,
                        _options.ClientId!,
                        _options.ClientSecret!),

                AuthType.AzureCli => new AzureCliCredential(),
                AuthType.VisualStudio => new VisualStudioCredential(),

                _ => throw new NotSupportedException(
                    $"Unsupported Key Vault auth type: {_options.AuthType}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create Key Vault credential using {AuthType}",
                _options.AuthType);

            throw new InvalidOperationException(
                $"Failed to create Key Vault credential using {_options.AuthType}",
                ex);
        }
    }
}
