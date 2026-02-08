using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Shared.AppConfiguration.Authentication;

public sealed class AppConfigurationCredentialFactory
    : IAppConfigurationCredentialFactory
{
    private readonly AppConfigurationAuthOptions _options;
    private readonly ILogger<AppConfigurationCredentialFactory> _logger;

    public AppConfigurationCredentialFactory(
        AppConfigurationAuthOptions options,
        ILogger<AppConfigurationCredentialFactory> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public TokenCredential CreateCredential()
    {
        try
        {
            _logger.LogInformation(
                "Creating App Configuration credential using {AuthType}",
                _options.AuthType);

            return _options.AuthType switch
            {
                AppConfigurationAuthType.Default =>
                    new DefaultAzureCredential(),

                AppConfigurationAuthType.ManagedIdentity =>
                    CreateManagedIdentityCredential(),

                AppConfigurationAuthType.ClientSecret =>
                    CreateClientSecretCredential(),

                AppConfigurationAuthType.AzureCli =>
                    new AzureCliCredential(),

                AppConfigurationAuthType.VisualStudio =>
                    new VisualStudioCredential(),

                _ => throw new NotSupportedException(
                    $"Auth type '{_options.AuthType}' is not supported.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create App Configuration credential");
            throw;
        }
    }

    private TokenCredential CreateManagedIdentityCredential()
    {
        _logger.LogDebug("Using ManagedIdentityCredential");

        return string.IsNullOrWhiteSpace(_options.ManagedIdentityClientId)
            ? new ManagedIdentityCredential()
            : new ManagedIdentityCredential(_options.ManagedIdentityClientId);
    }

    private TokenCredential CreateClientSecretCredential()
    {
        if (string.IsNullOrWhiteSpace(_options.TenantId) ||
            string.IsNullOrWhiteSpace(_options.ClientId) ||
            string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            throw new InvalidOperationException(
                "ClientSecret authentication requires TenantId, ClientId, and ClientSecret.");
        }

        _logger.LogDebug("Using ClientSecretCredential");

        return new ClientSecretCredential(
            _options.TenantId,
            _options.ClientId,
            _options.ClientSecret);
    }
}
