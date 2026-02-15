using Azure.Monitor.OpenTelemetry.AspNetCore;
using ConfigManagement.Sync.Orchestrator.Functions.Context;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ConfigManagement.Sync.Orchestrator.Functions.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetry(
        this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .UseAzureMonitor()
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource("ConfigManagement.Shared.ServiceBus")
                    .AddSource("ConfigManagement.Shared.KeyVault")
                    .AddSource("ConfigManagement.Shared.AppConfiguration")
                    .AddSource("ConfigManagement.Sync.Orchestrator") // Your function source
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
                        serviceVersion: typeof(TelemetryExtensions).Assembly.GetName().Version?.ToString())
                    .AddAttributes(metadataContext.ToResourceAttributes()));
        });

        return services;
    }
}





