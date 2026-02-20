using Aep.Cmo.Event.Orchestrator.Application.Services;
using Aep.Cmo.Event.Orchestrator.Functions.Extensions;
using Aep.Cmo.Event.Orchestrator.Application.Interfaces;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Aep.Cmo.Event.Orchestrator.Application.Extensions;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddServiceMetadata(builder.Configuration)
    .AddTelemetry(builder.Configuration)
    .AddAppConfiguration(builder.Configuration)
    .AddKeyVault(builder.Configuration)
    .AddServiceBus(builder.Configuration)
    .AddScoped<IAppConfigurationEventService, AppConfigurationEventService>()
    .AddScoped<IKeyVaultEventService, KeyVaultEventService>();

await builder.Build().RunAsync();
