using Azure.Core;
using Azure.Identity;
using ConfigManagement.Shared.ServiceBus.Enums;
using ConfigManagement.Shared.ServiceBus.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Shared.ServiceBus.Authentication;

/// <summary>
/// Factory responsible for creating <see cref="TokenCredential"/> instances
/// used to authenticate against Azure Service Bus.
/// </summary>
/// <remarks>
/// This implementation selects the appropriate Azure Identity credential
/// based on the configured <see cref="AuthType"/> provided via
/// <see cref="IServiceBusAuthOptions"/>.
///
/// Supported authentication mechanisms include:
/// <list type="bullet">
/// <item><see cref="AuthType.Default"/> – Uses <see cref="DefaultAzureCredential"/>.</item>
/// <item><see cref="AuthType.ManagedIdentity"/> – Uses <see cref="ManagedIdentityCredential"/>.</item>
/// <item><see cref="AuthType.ClientSecret"/> – Uses <see cref="ClientSecretCredential"/>.</item>
/// <item><see cref="AuthType.AzureCli"/> – Uses <see cref="AzureCliCredential"/>.</item>
/// <item><see cref="AuthType.VisualStudio"/> – Uses <see cref="VisualStudioCredential"/>.</item>
/// </list>
/// 
/// The factory centralizes credential creation logic to:
/// <list type="bullet">
/// <item><description>Encapsulate authentication strategy selection.</description></item>
/// <item><description>Improve testability through abstraction.</description></item>
/// <item><description>Provide consistent logging and error handling.</description></item>
/// </list>
/// </remarks>
public sealed class ServiceBusCredentialFactory
    : IServiceBusCredentialFactory
{
    private readonly IServiceBusAuthOptions _options;
    private readonly ILogger<ServiceBusCredentialFactory> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBusCredentialFactory"/> class.
    /// </summary>
    /// <param name="options">
    /// Authentication configuration used to determine which credential to create.
    /// </param>
    /// <param name="logger">
    /// Logger used for diagnostic and error logging.
    /// </param>
    public ServiceBusCredentialFactory(
        IServiceBusAuthOptions options,
        ILogger<ServiceBusCredentialFactory> logger)
    {
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Creates a <see cref="TokenCredential"/> based on the configured <see cref="AuthType"/>.
    /// </summary>
    /// <returns>
    /// A configured <see cref="TokenCredential"/> implementation.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the configured <see cref="AuthType"/> is not supported.
    /// </exception>
    /// <exception cref="Exception">
    /// Re-throws any exception encountered during credential creation
    /// after logging the error.
    /// </exception>
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

            throw; // Important: preserve the original stack trace
        }
    }
}
