using Azure.Monitor.OpenTelemetry.Exporter;
using ConfigManagement.Shared.AppConfiguration;
using ConfigManagement.Shared.AppConfiguration.Interfaces;
using ConfigManagement.Shared.AppConfiguration.Options;
using ConfigManagement.Shared.KeyVault.Authentication;
using ConfigManagement.Shared.KeyVault.Interfaces;
using ConfigManagement.Shared.KeyVault.Options;
using ConfigManagement.Sync.Orchestrator.Application;
using ConfigManagement.Sync.Orchestrator.Application.Context;
using ConfigManagement.Sync.Orchestrator.Application.Interfaces;
using ConfigManagement.Sync.Orchestrator.Application.Options;
using ConfigManagement.Sync.Orchestrator.Application.Orchestration;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;
using ConfigManagement.Sync.Orchestrator.Infrastructure.KeyVault;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Options;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using AppCfgAuthType = ConfigManagement.Shared.AppConfiguration.Enums.AuthType;
using KeyVaultAuthType = ConfigManagement.Shared.KeyVault.Enums.AuthType;

/// <summary>
/// Configures the Config Management Sync Orchestrator Azure Function,
/// including distributed tracing via OpenTelemetry.
/// </summary>
var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();


// -------------------------------------------------
// Service Metadata
// -------------------------------------------------

var metadata = builder.Configuration
    .Get<ServiceMetaDataOptions>()
    ?? throw new InvalidOperationException("Service metadata missing.");

var metadataContext = new ServiceMetadataContext(metadata);

// -------------------------------------------------
// OpenTelemetry Configuration
// -------------------------------------------------

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(metadataContext.ServiceName)
                    .AddAttributes(metadataContext.ToResourceAttributes()))
            .AddSource("ConfigManagement.Shared.ServiceBus")
            .AddSource("ConfigManagement.Shared.KeyVault")
            .AddSource("ConfigManagement.Shared.AppConfiguration")
            .AddHttpClientInstrumentation()
            .AddAzureMonitorTraceExporter();
    });

// -------------------------------------------------
// App Configuration
// -------------------------------------------------

builder.Services
    .AddOptions<AppConfigurationOptions>()
    .Bind(builder.Configuration.GetSection("AppConfiguration"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint),
        "App Configuration Endpoint is missing")
    .ValidateOnStart();

builder.Services.AddSingleton<IAppConfigurationOptions>(sp =>
    sp.GetRequiredService<IOptions<AppConfigurationOptions>>().Value);

builder.Services
    .AddOptions<AppConfigurationAuthOptions>()
    .Bind(builder.Configuration.GetSection("AppConfiguration:Auth"))
    .Validate(o => o.AuthType switch
    {
        AppCfgAuthType.ClientSecret =>
            !string.IsNullOrWhiteSpace(o.TenantId) &&
            !string.IsNullOrWhiteSpace(o.ClientId) &&
            !string.IsNullOrWhiteSpace(o.ClientSecret),
        AppCfgAuthType.ManagedIdentity => true,
        AppCfgAuthType.Default => true,
        AppCfgAuthType.AzureCli => true,
        AppCfgAuthType.VisualStudio => true,
        _ => false
    }, "Invalid App Configuration authentication configuration")
    .ValidateOnStart();

builder.Services.AddSingleton<IAppConfigurationAuthOptions>(sp =>
    sp.GetRequiredService<IOptions<AppConfigurationAuthOptions>>().Value);

builder.Services.AddSingleton<IAppConfigurationCredentialFactory, AppConfigurationCredentialFactory>();
builder.Services.AddScoped<IAppConfigurationClient, AppConfigurationClient>();


// -------------------------------------------------
// Key Vault
// -------------------------------------------------

builder.Services
    .AddOptions<LocalKeyVaultOptions>()
    .Bind(builder.Configuration.GetSection("KeyVault:Spoke"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint),
        "Local KeyVault Endpoint is missing")
    .ValidateOnStart();

builder.Services
    .AddOptions<HubKeyVaultOptions>()
    .Bind(builder.Configuration.GetSection("KeyVault:Hub"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint),
        "Hub KeyVault Endpoint is missing")
    .ValidateOnStart();

builder.Services.AddSingleton<IHubKeyVaultOptions>(sp =>
    sp.GetRequiredService<IOptions<HubKeyVaultOptions>>().Value);

builder.Services.AddSingleton<ILocalKeyVaultOptions>(sp =>
    sp.GetRequiredService<IOptions<LocalKeyVaultOptions>>().Value);

builder.Services
    .AddOptions<KeyVaultAuthOptions>()
    .Bind(builder.Configuration.GetSection("KeyVault:Auth"))
    .Validate(o =>
        o.AuthType != KeyVaultAuthType.ClientSecret ||
        (!string.IsNullOrWhiteSpace(o.TenantId) &&
         !string.IsNullOrWhiteSpace(o.ClientId) &&
         !string.IsNullOrWhiteSpace(o.ClientSecret)),
        "ClientSecret auth requires TenantId, ClientId, and ClientSecret")
    .ValidateOnStart();

builder.Services.AddSingleton<IKeyVaultAuthOptions>(sp =>
    sp.GetRequiredService<IOptions<KeyVaultAuthOptions>>().Value);

builder.Services.AddSingleton<IKeyVaultCredentialFactory, KeyVaultCredentialFactory>();
builder.Services.AddScoped<ILocalKeyVaultSecretClient, LocalKeyVaultSecretClient>();
builder.Services.AddScoped<IHubKeyVaultSecretClient, HubKeyVaultSecretClient>();


// -------------------------------------------------
// Application Layer
// -------------------------------------------------

builder.Services.AddScoped<IConfigSyncHandler, ConfigSyncHandler>();
builder.Services.AddScoped<IResultOrchestrator, ResultOrchestrator>();


builder.Build().Run();
