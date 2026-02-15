using ConfigManagement.Shared.AppConfiguration.Extensions;
using ConfigManagement.Sync.Orchestrator.Application.Interfaces;
using ConfigManagement.Sync.Orchestrator.Application.Services;
using ConfigManagement.Sync.Orchestrator.Functions.Extensions;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddServiceMetadata(builder.Configuration);
builder.Services.AddTelemetry();
builder.Services.AddAppConfiguration(builder.Configuration);
builder.Services.AddKeyVault(builder.Configuration);
builder.Services.AddScoped<IAppConfigurationSyncService, AppConfigurationSyncService>();
builder.Services.AddScoped<IKeyVaultSyncService, KeyVaultSyncService>();

await builder.Build().RunAsync();
