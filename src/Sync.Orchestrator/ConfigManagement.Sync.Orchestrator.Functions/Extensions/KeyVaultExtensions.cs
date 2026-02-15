using ConfigManagement.Shared.KeyVault;
using ConfigManagement.Shared.KeyVault.Authentication;
using ConfigManagement.Shared.KeyVault.Enums;
using ConfigManagement.Shared.KeyVault.Interfaces;
using ConfigManagement.Shared.KeyVault.Options;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;
using ConfigManagement.Sync.Orchestrator.Infrastructure.KeyVault;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ConfigManagement.Sync.Orchestrator.Functions.Extensions;

public static class KeyVaultExtensions
{
    public static IServiceCollection AddKeyVault(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<KeyVaultOptions>()
            .Bind(configuration.GetSection("KeyVault:Spoke"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint))
            .ValidateOnStart();

        services
            .AddOptions<HubKeyVaultOptions>()
            .Bind(configuration.GetSection("KeyVault:Hub"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint))
            .ValidateOnStart();

        services
            .AddOptions<KeyVaultAuthOptions>()
            .Bind(configuration.GetSection("KeyVault:Auth"))
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

        services.AddSingleton<IKeyVaultAuthOptions>(sp =>
            sp.GetRequiredService<IOptions<KeyVaultAuthOptions>>().Value);

        services.AddSingleton<IKeyVaultCredentialFactory, KeyVaultCredentialFactory>();
        services.AddScoped<IKeyVaultSecretClient, KeyVaultSecretClient>();
        services.AddScoped<IHubKeyVaultSecretClient, HubKeyVaultSecretClient>();

        return services;
    }
}

