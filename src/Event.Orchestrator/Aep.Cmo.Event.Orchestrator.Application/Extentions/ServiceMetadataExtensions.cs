using Aep.Cmo.Event.Orchestrator.Application.Context;
using Aep.Cmo.Event.Orchestrator.Application.Interfaces;
using Aep.Cmo.Event.Orchestrator.Application.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aep.Cmo.Event.Orchestrator.Application.Extensions;

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