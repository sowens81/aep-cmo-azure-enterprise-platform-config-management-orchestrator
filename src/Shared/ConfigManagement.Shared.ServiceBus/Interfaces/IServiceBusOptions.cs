namespace ConfigManagement.Shared.ServiceBus.Interfaces;

/// <summary>
/// Defines configuration required to connect to Azure Service Bus.
/// </summary>
/// <remarks>
/// This interface is typically implemented by an options class that is bound
/// from application configuration (e.g., appsettings.json or environment variables).
/// It provides the fully qualified namespace endpoint used to establish
/// a connection to Azure Service Bus.
/// </remarks>
public interface IServiceBusOptions
{
    /// <summary>
    /// Gets the fully qualified Azure Service Bus namespace endpoint.
    /// </summary>
    /// <remarks>
    /// The endpoint should be in the format:
    /// <c>your-namespace.servicebus.windows.net</c>
    /// and must not include protocol prefixes such as <c>https://</c>.
    /// </remarks>
    string Endpoint { get; }
}
