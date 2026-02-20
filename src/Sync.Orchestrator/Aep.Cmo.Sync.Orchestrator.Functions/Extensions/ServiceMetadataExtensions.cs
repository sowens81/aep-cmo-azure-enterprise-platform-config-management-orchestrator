using Aep.Cmo.Sync.Orchestrator.Functions.Context;
using Aep.Cmo.Sync.Orchestrator.Functions.Interfaces;
using Aep.Cmo.Sync.Orchestrator.Functions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aep.Cmo.Sync.Orchestrator.Functions.Extensions;

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
    public static IServiceCollection AddServiceMetadata(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var metadata = configuration.Get<ServiceMetaDataOptions>()
            ?? throw new InvalidOperationException("Service metadata missing.");

        services.AddSingleton<IServiceMetadata>(
            new ServiceMetadataContext(metadata));

        return services;
    }
}
