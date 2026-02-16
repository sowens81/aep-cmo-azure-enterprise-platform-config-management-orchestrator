using Aep.Cmo.Shared.AppConfiguration;
using Aep.Cmo.Shared.AppConfiguration.Authentication;
using Aep.Cmo.Shared.AppConfiguration.Enums;
using Aep.Cmo.Shared.AppConfiguration.Interfaces;
using Aep.Cmo.Shared.AppConfiguration.Options;
using Aep.Cmo.Sync.Orchestrator.Infrastructure.AppConfiguration;
using Aep.Cmo.Sync.Orchestrator.Infrastructure.Interfaces;
using Aep.Cmo.Sync.Orchestrator.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aep.Cmo.Sync.Orchestrator.Functions.Extensions;

public static class AppConfigurationExtensions
{
    public static IServiceCollection AddAppConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<AppConfigurationOptions>()
            .Bind(configuration.GetSection("AppConfiguration:Hub"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint),
                "App Configuration Endpoint is missing")
            .ValidateOnStart();

        services
            .AddOptions<HubAppConfigurationOptions>()
            .Bind(configuration.GetSection("AppConfiguration:Spoke"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint),
                "App Configuration Endpoint is missing")
            .ValidateOnStart();

        services
            .AddOptions<AppConfigurationAuthOptions>()
            .Bind(configuration.GetSection("AppConfiguration:Auth"))
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

        services.AddSingleton<IAppConfigurationOptions>(sp =>
        sp.GetRequiredService<IOptions<AppConfigurationOptions>>().Value);

        services.AddSingleton<IHubAppConfigurationOptions>(sp =>
            sp.GetRequiredService<IOptions<HubAppConfigurationOptions>>().Value);

        services.AddSingleton<IAppConfigurationAuthOptions>(sp =>
            sp.GetRequiredService<IOptions<AppConfigurationAuthOptions>>().Value);

        services.AddSingleton<IAppConfigurationCredentialFactory, AppConfigurationCredentialFactory>();
        services.AddScoped<IAppConfigurationClient, AppConfigurationClient>();
        services.AddScoped<IHubAppConfigurationClient, HubAppConfigurationClient>();

        return services;
    }
}


