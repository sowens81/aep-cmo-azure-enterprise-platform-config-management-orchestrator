using Aep.Cmo.Sync.Orchestrator.Application.Interfaces;
using Aep.Cmo.Sync.Orchestrator.Application.Services;
using Aep.Cmo.Sync.Orchestrator.Functions.Extensions;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddServiceMetadata(builder.Configuration)
    .AddTelemetry(builder.Configuration)
    .AddAppConfiguration(builder.Configuration)
    .AddKeyVault(builder.Configuration)
    .AddScoped<IAppConfigurationSyncService, AppConfigurationSyncService>()
    .AddScoped<IKeyVaultSyncService, KeyVaultSyncService>();

await builder.Build().RunAsync();
