using ConfigManagement.Shared.AppConfiguration.Enums;
using ConfigManagement.Shared.AppConfiguration.Interfaces;
using ConfigManagement.Shared.AppConfiguration.Options;
using ConfigManagement.Shared.KeyVault;
using ConfigManagement.Shared.KeyVault.Authentication;
using ConfigManagement.Shared.KeyVault.Interfaces;
using ConfigManagement.Shared.KeyVault.Options;
using ConfigManagement.Sync.Orchestrator.Infrastructure.AppConfiguration;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;
using ConfigManagement.Sync.Orchestrator.Infrastructure.KeyVault;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ConfigManagement.Shared.AppConfiguration.Extensions;

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

        services.AddSingleton<IAppConfigurationAuthOptions>(sp =>
            sp.GetRequiredService<IOptions<AppConfigurationAuthOptions>>().Value);

        services.AddSingleton<IAppConfigurationCredentialFactory, AppConfigurationCredentialFactory>();
        services.AddScoped<IAppConfigurationClient, AppConfigurationClient>();
        services.AddScoped<IHubAppConfigurationClient, HubAppConfigurationClient>();

        return services;
    }
}


