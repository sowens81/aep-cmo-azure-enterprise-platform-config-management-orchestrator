namespace ConfigManagement.Sync.Orchestrator.Application.Options;

/// <summary>
/// Represents immutable service-level metadata used for
/// distributed tracing, logging, and environment identification.
/// </summary>
/// <remarks>
/// All properties are required and must be supplied via configuration.
/// Startup validation ensures they are not null or empty.
/// </remarks>
public sealed class ServiceMetaDataOptions
{
    /// <summary>
    /// The organisation identifier.
    /// </summary>
    public required string Organisation { get; init; }

    /// <summary>
    /// The region in which the service is deployed.
    /// </summary>
    public required string Region { get; init; }

    /// <summary>
    /// The environment tier (e.g., Dev, Test, Prod).
    /// </summary>
    public required string EnvironmentTier { get; init; }

    /// <summary>
    /// The environment name (e.g., UK-Prod-01).
    /// </summary>
    public required string EnvironmentName { get; init; }

    /// <summary>
    /// The logical service name.
    /// </summary>
    public required string ServiceName { get; init; }
}
