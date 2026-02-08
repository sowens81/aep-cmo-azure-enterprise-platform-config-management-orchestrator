using ConfigManagement.Shared.AppConfiguration;
using ConfigManagement.Shared.AppConfiguration.Authentication;
using ConfigManagement.Shared.AppConfiguration.Options;
using ConfigManagement.Shared.KeyVault;
using ConfigManagement.Shared.KeyVault.Authentication;
using ConfigManagement.Shared.KeyVault.Options;
using ConfigManagement.Shared.ServiceBus;
using ConfigManagement.Shared.ServiceBus.Authentication;
using ConfigManagement.Sync.Orchestrator.Application;
using ConfigManagement.Sync.Orchestrator.Application.Interfaces;
using ConfigManagement.Sync.Orchestrator.Application.Orchestration;
using ConfigManagement.Sync.Orchestrator.Functions.Metadata;
using ConfigurationSyncOrchestrator.Domain.Factories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// -------------------------------------------------
// Telemetry
// -------------------------------------------------
builder
    .Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// -------------------------------------------------
// Environment + Credentials
// -------------------------------------------------
builder.Services.AddSingleton<ConfigFactory>();

// -------------------------------------------------
// Service Bus (Result publishing)
// -------------------------------------------------
builder.Services.AddSingleton<IServiceBusCredentialFactory>(sp =>
{
    var config = sp.GetRequiredService<ConfigFactory>();
    var logger = sp.GetRequiredService<ILogger<ServiceBusCredentialFactory>>();
    var options = config.ServiceBusOptions;
    return new ServiceBusCredentialFactory(options, logger);
});

builder.Services.AddSingleton<IResultPublisher>(sp =>
{
    var config = sp.GetRequiredService<ConfigFactory>();
    var logger = sp.GetRequiredService<ILogger<ResultTopicPublisher>>();
    var factory = sp.GetRequiredService<ServiceBusCredentialFactory>();

    return new ResultTopicPublisher(config.ServiceBusEventResultTopic, factory, logger);
});

// -------------------------------------------------
// Application layer
// -------------------------------------------------
builder.Services.AddSingleton<IConfigSyncHandler, ConfigSyncHandler>();

builder.Services.AddSingleton<IServiceMetadata>(sp =>
{
    var config = sp.GetRequiredService<ConfigFactory>();
    return new ConfigFactoryServiceMetadata(config);
});

builder.Services.AddSingleton<ISyncResultOrchestrator, SyncResultOrchestrator>();

// -------------------------------------------------
// App Configuration
// -------------------------------------------------
builder.Services.AddSingleton<IAppConfigurationCredentialFactory>(sp =>
{
    var config = sp.GetRequiredService<ConfigFactory>();
    var logger = sp.GetRequiredService<ILogger<AppConfigurationCredentialFactory>>();
    var authOptions = config.AppConfigOptions;
    return new AppConfigurationCredentialFactory(authOptions, logger);
});

builder.Services.AddSingleton<IAppConfigurationClient>(sp =>
{
    var config = sp.GetRequiredService<ConfigFactory>();
    var logger = sp.GetRequiredService<ILogger<AppConfigurationClient>>();
    var credentialFactory = sp.GetRequiredService<IAppConfigurationCredentialFactory>();

    var options = new AppConfigurationOptions { Endpoint = config.AppConfigEndpoint };

    return new AppConfigurationClient(options, credentialFactory, logger);
});

// -------------------------------------------------
// Key Vault clients (Hub + Local)
// IMPORTANT: keep them distinct
// -------------------------------------------------
builder.Services.AddSingleton<IKeyVaultCredentialFactory>(sp =>
{
    var config = sp.GetRequiredService<ConfigFactory>();
    var logger = sp.GetRequiredService<ILogger<KeyVaultCredentialFactory>>();

    var options = config.KeyVaultOptions;

    return new KeyVaultCredentialFactory(options, logger);
});

builder.Services.AddSingleton<ILocalKeyVaultSecretClient>(sp =>
{
    var config = sp.GetRequiredService<ConfigFactory>();
    var logger = sp.GetRequiredService<ILogger<KeyVaultSecretClient>>();
    var credentialFactory = sp.GetRequiredService<IKeyVaultCredentialFactory>();

    var options = new KeyVaultOptions { VaultUri = config.LocalKeyVaultUri };

    return new KeyVaultSecretClient(options, credentialFactory, logger);
});

builder.Services.AddSingleton<IHubKeyVaultSecretClient>(sp =>
{
    var config = sp.GetRequiredService<ConfigFactory>();
    var logger = sp.GetRequiredService<ILogger<KeyVaultSecretClient>>();
    var credentialFactory = sp.GetRequiredService<IKeyVaultCredentialFactory>();

    var options = new KeyVaultOptions { VaultUri = config.HubKeyVaultUri };

    return new KeyVaultSecretClient(options, credentialFactory, logger);
});

builder.Build().Run();
