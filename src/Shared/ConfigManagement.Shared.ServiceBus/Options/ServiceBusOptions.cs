using ConfigManagement.Shared.ServiceBus.Interfaces;

namespace ConfigManagement.Shared.ServiceBus.Options;

/// <summary>
/// Represents configuration options required to connect to Azure Service Bus.
/// </summary>
/// <remarks>
/// This options class is typically bound from configuration (e.g., appsettings.json)
/// and provides the fully qualified namespace endpoint used to establish
/// a connection to Azure Service Bus.
///
/// All properties use <c>init</c> accessors to ensure immutability after
/// configuration binding.
/// </remarks>
public class ServiceBusOptions : IServiceBusOptions
{
    /// <summary>
    /// Gets the fully qualified Azure Service Bus namespace endpoint.
    /// </summary>
    /// <remarks>
    /// This should be in the format:
    /// <c>your-namespace.servicebus.windows.net</c>
    /// and must not include protocol prefixes (e.g., https://).
    /// </remarks>
    public string Endpoint { get; init; } = default!;
}
