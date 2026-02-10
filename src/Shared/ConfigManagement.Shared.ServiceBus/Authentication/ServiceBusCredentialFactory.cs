using Azure.Core;
using Azure.Identity;
using ConfigManagement.Shared.ServiceBus.Enums;
using ConfigManagement.Shared.ServiceBus.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Shared.ServiceBus.Authentication;

public sealed class ServiceBusCredentialFactory
    : IServiceBusCredentialFactory
{
    private readonly IServiceBusAuthOptions _options;
    private readonly ILogger<ServiceBusCredentialFactory> _logger;

    public ServiceBusCredentialFactory(
        IServiceBusAuthOptions options,
        ILogger<ServiceBusCredentialFactory> logger)
    {
        _options = options;
        _logger = logger;
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
                    $"Unsupported auth type: {_options.AuthType}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create App Configuration credential using {AuthType}",
                _options.AuthType);

            throw; // important: preserve stack trace
        }
    }
}

