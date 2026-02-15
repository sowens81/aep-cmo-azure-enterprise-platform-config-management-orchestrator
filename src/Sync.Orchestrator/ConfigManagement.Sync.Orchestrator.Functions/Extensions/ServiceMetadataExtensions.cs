using ConfigManagement.Sync.Orchestrator.Functions.Context;
using ConfigManagement.Sync.Orchestrator.Functions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigManagement.Sync.Orchestrator.Functions.Extensions;

/// <summary>
/// Provides extension methods for registering service metadata into the dependency injection container.
/// </summary>
public static class ServiceMetadataExtensions
{
    /// <summary>
    /// Registers <see cref="ServiceMetadataContext"/> as a singleton using configuration-bound metadata.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> used to register application services.
    /// </param>
    /// <param name="configuration">
    /// The <see cref="IConfiguration"/> instance used to bind <see cref="ServiceMetaDataOptions"/>.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance to allow method chaining.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the required service metadata configuration section is missing or cannot be bound.
    /// </exception>
    /// <remarks>
    /// This method:
    /// <list type="bullet">
    /// <item>
    /// Binds configuration values to <see cref="ServiceMetaDataOptions"/>.
    /// </item>
    /// <item>
    /// Creates a <see cref="ServiceMetadataContext"/> instance using the bound options.
    /// </item>
    /// <item>
    /// Registers the context as a singleton to ensure consistent metadata usage
    /// across the application lifetime.
    /// </item>
    /// </list>
    /// 
    /// The registered <see cref="ServiceMetadataContext"/> is typically used to enrich
    /// telemetry resources, logging, and other cross-cutting concerns with
    /// service-specific metadata.
    /// </remarks>
    public static IServiceCollection AddServiceMetadata(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var metadata = configuration.Get<ServiceMetaDataOptions>()
            ?? throw new InvalidOperationException("Service metadata missing.");

        var context = new ServiceMetadataContext(metadata);

        services.AddSingleton(context);

        return services;
    }
}
