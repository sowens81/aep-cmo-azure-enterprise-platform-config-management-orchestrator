namespace ConfigManagement.Sync.Orchestrator.Functions.Interfaces;

/// <summary>
/// Represents immutable metadata describing the current service instance.
/// </summary>
/// <remarks>
/// This metadata is used for:
/// <list type="bullet">
/// <item>Distributed tracing (OpenTelemetry resource attributes)</item>
/// <item>Structured logging scopes</item>
/// <item>Environment identification</item>
/// </list>
/// Implementations must guarantee that all values are non-null and non-empty.
/// </remarks>
public interface IServiceMetadata
{
    /// <summary>
    /// Gets the organisation identifier.
    /// </summary>
    string Organisation { get; }

    /// <summary>
    /// Gets the deployment region.
    /// </summary>
    string Region { get; }

    /// <summary>
    /// Gets the environment tier (e.g., Dev, Test, Prod).
    /// </summary>
    string EnvironmentTier { get; }

    /// <summary>
    /// Gets the environment name.
    /// </summary>
    string EnvironmentName { get; }

    /// <summary>
    /// Gets the logical service name.
    /// </summary>
    string ServiceName { get; }

    /// <summary>
    /// Returns the metadata as OpenTelemetry resource attributes.
    /// </summary>
    /// <returns>
    /// A dictionary of key/value pairs suitable for use with
    /// <c>ResourceBuilder.AddAttributes</c>.
    /// </returns>
    IReadOnlyDictionary<string, object> ToResourceAttributes();
}
