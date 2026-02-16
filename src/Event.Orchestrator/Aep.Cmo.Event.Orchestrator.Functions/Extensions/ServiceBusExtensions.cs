using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;
using Aep.Cmo.Event.Orchestrator.Infrastructure.Options;
using Aep.Cmo.Event.Orchestrator.Infrastructure.ServiceBus;
using Aep.Cmo.Shared.ServiceBus.Authentication;
using Aep.Cmo.Shared.ServiceBus.Enums;
using Aep.Cmo.Shared.ServiceBus.Interfaces;
using Aep.Cmo.Shared.ServiceBus.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aep.Cmo.Event.Orchestrator.Functions.Extensions;

public static class ServiceBusExtensions
{
    public static IServiceCollection AddServiceBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services
            .AddOptions<AppConfigServiceBusTopicOptions>()
            .Bind(configuration.GetSection("ServiceBus:Topics:AppConfigSync"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.TopicName),
                "Service bus topic name is missing for AppConfigSync")
            .ValidateOnStart();

        services
            .AddOptions<KeyVaultServiceBusTopicOptions>()
            .Bind(configuration.GetSection("ServiceBus:Topics:KeyVaultSync"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.TopicName),
                "Service bus topic name is missing for KeyVaultSync")
            .ValidateOnStart();

        services
            .AddOptions<ServiceBusOptions>()
            .Bind(configuration.GetSection("ServiceBus"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint),
                "Service Bus Endpoint is missing")
            .ValidateOnStart();

        services
            .AddOptions<ServiceBusAuthOptions>()
            .Bind(configuration.GetSection("ServiceBus:Auth"))
            .Validate(o => o.AuthType switch
            {
                AuthType.ClientSecret =>
                    !string.IsNullOrWhiteSpace(o.TenantId) &&
                    !string.IsNullOrWhiteSpace(o.ClientId) &&
                    !string.IsNullOrWhiteSpace(o.ClientSecret),
                AuthType.ManagedIdentity => true,
                AuthType.Default => true,
                AuthType.AzureCli => true,
                AuthType.VisualStudio => true,
                _ => false
            })
            .ValidateOnStart();

        services.AddSingleton<IAppConfigServiceBusTopicOptions>(sp =>
            sp.GetRequiredService<IOptions<AppConfigServiceBusTopicOptions>>().Value);

        services.AddSingleton<IKeyVaultServiceBusTopicOptions>(sp =>
            sp.GetRequiredService<IOptions<KeyVaultServiceBusTopicOptions>>().Value);

        services.AddSingleton<IServiceBusOptions>(sp =>
            sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value);

        services.AddSingleton<IServiceBusAuthOptions>(sp =>
            sp.GetRequiredService<IOptions<ServiceBusAuthOptions>>().Value);

        services.AddSingleton<IServiceBusCredentialFactory, ServiceBusCredentialFactory>();

        services.AddScoped(typeof(IAppConfigTopicPublisherClient<>), typeof(AppConfigTopicPublisherClient<>));
        services.AddScoped(typeof(IKeyVaultTopicPublisherClient<>), typeof(KeyVaultTopicPublisherClient<>));

        return services;
    }
}



