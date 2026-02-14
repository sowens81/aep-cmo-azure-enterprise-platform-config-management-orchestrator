using System.Diagnostics;

namespace ConfigManagement.Shared.KeyVault.OpenTelemetry;

/// <summary>
/// Provides a centralized <see cref="ActivitySource"/> for the Key Vault shared library.
/// </summary>
/// <remarks>
/// This <see cref="ActivitySource"/> is intended exclusively for Key Vault
/// operations (e.g., retrieving, setting, or deleting secrets).
///
/// The consuming application must register <see cref="ActivitySourceName"/>
/// when configuring OpenTelemetry in order to collect spans created by this library:
///
/// <code>
/// services.AddOpenTelemetry()
///     .WithTracing(tracing =>
///     {
///         tracing.AddSource(Telemetry.ActivitySourceName);
///     });
/// </code>
/// </remarks>
public static class Telemetry
{
    /// <summary>
    /// The name of the <see cref="ActivitySource"/> used by the Key Vault shared library.
    /// </summary>
    /// <remarks>
    /// This constant should be referenced during OpenTelemetry configuration
    /// to ensure tracing data is collected.
    /// </remarks>
    public const string ActivitySourceName = "ConfigManagement.Shared.KeyVault";

    /// <summary>
    /// The shared <see cref="ActivitySource"/> instance used to create
    /// tracing activities for Key Vault operations.
    /// </summary>
    /// <example>
    /// <code>
    /// using var activity = Telemetry.Source.StartActivity("KeyVault.GetSecret");
    /// </code>
    /// </example>
    public static readonly ActivitySource Source =
        new(ActivitySourceName);
}
