using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Shared.ServiceBus.Authentication;

public sealed class ServiceBusCredentialFactory : IServiceBusCredentialFactory
{
    private readonly ServiceBusAuthOptions _options;
    private readonly ILogger<ServiceBusCredentialFactory> _logger;

    public ServiceBusCredentialFactory(
        ServiceBusAuthOptions options,
        ILogger<ServiceBusCredentialFactory> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ServiceBusClient CreateClient()
    {
        try
        {
            _logger.LogInformation(
                "Creating ServiceBusClient using auth type {AuthType}",
                _options.AuthType);

            if (string.IsNullOrWhiteSpace(_options.FullyQualifiedNamespace))
            {
                throw new InvalidOperationException(
                    "Service Bus FullyQualifiedNamespace is not configured.");
            }

            TokenCredential credential = CreateCredential();

            var clientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpTcp
                // You can tune retry options here if needed
            };

            var client = new ServiceBusClient(
                _options.FullyQualifiedNamespace,
                credential,
                clientOptions);

            _logger.LogInformation("ServiceBusClient created successfully");

            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create ServiceBusClient");
            throw;
        }
    }

    private TokenCredential CreateCredential()
    {
        return _options.AuthType switch
        {
            ServiceBusAuthType.Default => new DefaultAzureCredential(),

            ServiceBusAuthType.ClientSecret => CreateClientSecretCredential(),

            ServiceBusAuthType.ManagedIdentity => CreateManagedIdentityCredential(),

            ServiceBusAuthType.VisualStudio => new VisualStudioCredential(),

            ServiceBusAuthType.AzureCli => new AzureCliCredential(),

            _ => throw new NotSupportedException(
                $"Authentication type '{_options.AuthType}' is not supported.")
        };
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

    private TokenCredential CreateManagedIdentityCredential()
    {
        _logger.LogDebug("Using ManagedIdentityCredential");

        return string.IsNullOrWhiteSpace(_options.ManagedIdentityClientId)
            ? new ManagedIdentityCredential()
            : new ManagedIdentityCredential(_options.ManagedIdentityClientId);
    }
}

