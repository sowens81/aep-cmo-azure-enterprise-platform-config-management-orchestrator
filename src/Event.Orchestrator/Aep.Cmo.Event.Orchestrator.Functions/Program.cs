using Aep.Cmo.Event.Orchestrator.Application.Services;
using Aep.Cmo.Event.Orchestrator.Functions.Extensions;
using Aep.Cmo.Event.Orchestrator.Application.Interfaces;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddServiceMetadata(builder.Configuration);
builder.Services.AddTelemetry(builder.Configuration);
builder.Services.AddAppConfiguration(builder.Configuration);
builder.Services.AddKeyVault(builder.Configuration);
builder.Services.AddServiceBus(builder.Configuration);
builder.Services.AddScoped<IAppConfigurationEventService, AppConfigurationEventService>();
builder.Services.AddScoped<IKeyVaultEventService, KeyVaultEventService>();

await builder.Build().RunAsync();
