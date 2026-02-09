using ConfigManagement.Shared.AppConfiguration;
using ConfigManagement.Shared.AppConfiguration.Interfaces;
using ConfigManagement.Shared.AppConfiguration.Options;
using ConfigManagement.Shared.KeyVault.Authentication;
using ConfigManagement.Shared.KeyVault.Interfaces;
using ConfigManagement.Shared.KeyVault.Options;
using ConfigManagement.Sync.Orchestrator.Application;
using ConfigManagement.Sync.Orchestrator.Application.Interfaces;
using ConfigManagement.Sync.Orchestrator.Application.Orchestration;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;
using ConfigManagement.Sync.Orchestrator.Infrastructure.KeyVault;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using AppCfgAuthType = ConfigManagement.Shared.AppConfiguration.Enums.AuthType;
using KeyVaultAuthType = ConfigManagement.Shared.KeyVault.Enums.AuthType;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder
    .Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

//// -------------------------------------------------
//// Service Bus (Result publishing)
//// -------------------------------------------------
//builder.Services.AddSingleton<IServiceBusCredentialFactory>(sp =>
//{
//    var config = sp.GetRequiredService<ConfigFactory>();
//    var logger = sp.GetRequiredService<ILogger<ServiceBusCredentialFactory>>();
//    var options = config.ServiceBusOptions;
//    return new ServiceBusCredentialFactory(options, logger);
//});

//builder.Services.AddSingleton<IResultPublisher>(sp =>
//{
//    var config = sp.GetRequiredService<ConfigFactory>();
//    var logger = sp.GetRequiredService<ILogger<ResultTopicPublisher>>();
//    var factory = sp.GetRequiredService<ServiceBusCredentialFactory>();

//    return new ResultTopicPublisher(config.ServiceBusEventResultTopic, factory, logger);
//});

// -------------------------------------------------
// App Configuration
// -------------------------------------------------
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
// Key Vault clients (Hub + Local)
// IMPORTANT: keep them distinct
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

// Expose via interfaces (validated at startup)
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


builder.Services.AddSingleton<ILocalKeyVaultOptions>(sp =>
    sp.GetRequiredService<IOptions<LocalKeyVaultOptions>>().Value);

builder.Services.AddSingleton<IHubKeyVaultOptions>(sp =>
    sp.GetRequiredService<IOptions<HubKeyVaultOptions>>().Value);

builder.Services.AddSingleton<IKeyVaultAuthOptions>(sp =>
    sp.GetRequiredService<IOptions<KeyVaultAuthOptions>>().Value);

builder.Services.AddSingleton<IKeyVaultCredentialFactory, KeyVaultCredentialFactory>();

builder.Services.AddScoped<ILocalKeyVaultSecretClient, LocalKeyVaultSecretClient>();
builder.Services.AddScoped<IHubKeyVaultSecretClient, HubKeyVaultSecretClient>();


// -------------------------------------------------
// Application layer
// -------------------------------------------------
builder.Services.AddSingleton<IConfigSyncHandler, ConfigSyncHandler>();

//builder.Services.AddSingleton<IServiceMetadata>(sp =>
//{
//    var config = sp.GetRequiredService<ConfigFactory>();
//    return new ConfigFactoryServiceMetadata(config);
//});

builder.Services.AddSingleton<ISyncResultOrchestrator, SyncResultOrchestrator>();


builder.Build().Run();
