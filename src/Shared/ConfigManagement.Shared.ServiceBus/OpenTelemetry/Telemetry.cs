using System.Diagnostics;

namespace ConfigManagement.Shared.ServiceBus.OpenTelemetry;

/// <summary>
/// Provides a centralized <see cref="ActivitySource"/> for the Service Bus shared library.
/// </summary>
/// <remarks>
/// This <see cref="ActivitySource"/> is intended exclusively for
/// Azure Service Bus operations such as publishing, receiving,
/// completing, or dead-lettering messages.
///
/// The consuming application must register <see cref="ActivitySourceName"/>
/// when configuring OpenTelemetry in order to collect spans created
/// by this library:
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
    /// The name of the <see cref="ActivitySource"/> used by the Service Bus shared library.
    /// </summary>
    /// <remarks>
    /// This constant must be referenced during OpenTelemetry configuration
    /// to enable tracing for Service Bus operations.
    /// </remarks>
    public const string ActivitySourceName = "ConfigManagement.Shared.ServiceBus";

    /// <summary>
    /// The shared <see cref="ActivitySource"/> instance used to create
    /// tracing activities for Service Bus operations.
    /// </summary>
    /// <example>
    /// <code>
    /// using var activity = Telemetry.Source.StartActivity("ServiceBus.Publish");
    /// </code>
    /// </example>
    public static readonly ActivitySource Source =
        new(ActivitySourceName);
}
