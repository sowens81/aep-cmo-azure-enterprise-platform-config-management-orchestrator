using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;
using Aep.Cmo.Event.Orchestrator.Infrastructure.KeyVault;
using Aep.Cmo.Event.Orchestrator.Infrastructure.Options;
using Aep.Cmo.Shared.KeyVault.Authentication;
using Aep.Cmo.Shared.KeyVault.Enums;
using Aep.Cmo.Shared.KeyVault.Interfaces;
using Aep.Cmo.Shared.KeyVault.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aep.Cmo.Event.Orchestrator.Functions.Extensions;

public static class KeyVaultExtensions
{
    public static IServiceCollection AddKeyVault(
        this IServiceCollection services,
        IConfiguration configuration)
    {
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


        services.AddSingleton<IHubKeyVaultOptions>(sp =>
            sp.GetRequiredService<IOptions<HubKeyVaultOptions>>().Value);

        services.AddSingleton<IKeyVaultAuthOptions>(sp =>
            sp.GetRequiredService<IOptions<KeyVaultAuthOptions>>().Value);

        services.AddSingleton<IKeyVaultCredentialFactory, KeyVaultCredentialFactory>();

        services.AddScoped<IHubKeyVaultSecretClient, HubKeyVaultSecretClient>();

        return services;
    }
}

