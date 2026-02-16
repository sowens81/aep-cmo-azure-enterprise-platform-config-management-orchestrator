using Azure.Monitor.OpenTelemetry.AspNetCore;
using Aep.Cmo.Event.Orchestrator.Functions.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Aep.Cmo.Event.Orchestrator.Functions.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var aiConnectionString =
            configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

        if (string.IsNullOrWhiteSpace(aiConnectionString))
        {
            // No App Insights configured (local dev) — skip Azure Monitor
            services.AddOpenTelemetry()
                .WithTracing(tracing =>
                {
                    tracing
                        .AddSource("Aep.Cmo.Shared.ServiceBus")
                        .AddSource("Aep.Cmo.Shared.KeyVault")
                        .AddSource("Aep.Cmo.Shared.AppConfiguration")
                        .AddSource("ConfigManagement.Sync.Orchestrator")
                        .AddHttpClientInstrumentation();
                });

            return services;
        }

        // Azure Monitor enabled (Azure environment)
        services.AddOpenTelemetry()
            .UseAzureMonitor(options =>
            {
                options.ConnectionString = aiConnectionString;
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource("Aep.Cmo.Shared.ServiceBus")
                    .AddSource("Aep.Cmo.Shared.KeyVault")
                    .AddSource("Aep.Cmo.Shared.AppConfiguration")
                    .AddSource("ConfigManagement.Sync.Orchestrator")
                    .AddHttpClientInstrumentation();
            });

        services.ConfigureOpenTelemetryTracerProvider((sp, builder) =>
        {
            var metadataContext =
                sp.GetRequiredService<ServiceMetadataContext>();

            builder.SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(
                        serviceName: metadataContext.ServiceName,
                        serviceVersion: typeof(TelemetryExtensions)
                            .Assembly
                            .GetName()
                            .Version?
                            .ToString())
                    .AddAttributes(metadataContext.ToResourceAttributes()));
        });

        return services;
    }
}
