using System.Diagnostics;

namespace Aep.Cmo.Shared.AppConfiguration.OpenTelemetry;

/// <summary>
/// Provides the <see cref="ActivitySource"/> for the
/// ConfigurationManagementSync Shared AppConfiguration package.
/// </summary>
/// <remarks>
/// The consuming application must register this source via:
///
/// <code>
/// tracing.AddSource(Telemetry.ActivitySourceName);
/// </code>
/// </remarks>
public static class Telemetry
{
    /// <summary>
    /// The ActivitySource name for AppConfiguration instrumentation.
    /// </summary>
    public const string ActivitySourceName =
        "Aep.Cmo.Shared.AppConfiguration";

    /// <summary>
    /// The ActivitySource instance used to create spans.
    /// </summary>
    public static readonly ActivitySource Source =
        new(ActivitySourceName);
}
