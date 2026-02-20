using Aep.Cmo.Sync.Orchestrator.Functions.Context;
using Aep.Cmo.Sync.Orchestrator.Functions.Interfaces;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Aep.Cmo.Sync.Orchestrator.Functions.Extensions;

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
            var metadata =
                sp.GetRequiredService<IServiceMetadata>();

            builder.SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(
                        serviceName: metadata.ServiceName,
                        serviceVersion: typeof(TelemetryExtensions)
                            .Assembly
                            .GetName()
                            .Version?
                            .ToString())
                    .AddAttributes(metadata.ToResourceAttributes()));
        });

        return services;
    }
}
