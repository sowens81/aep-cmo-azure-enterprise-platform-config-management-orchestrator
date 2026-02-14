using Azure.Core;

namespace ConfigManagement.Shared.ServiceBus.Interfaces;

/// <summary>
/// Defines a factory responsible for creating <see cref="TokenCredential"/> instances
/// used to authenticate against Azure Service Bus.
/// </summary>
/// <remarks>
/// Implementations of this interface encapsulate the logic required to create
/// the appropriate credential based on configuration (e.g., default credential chain,
/// client secret, managed identity).
///
/// This abstraction enables:
/// <list type="bullet">
/// <item>
/// <description>Centralized authentication configuration.</description>
/// </item>
/// <item>
/// <description>Separation of concerns between authentication and messaging logic.</description>
/// </item>
/// <item>
/// <description>Improved testability through mocking of credential creation.</description>
/// </item>
/// </list>
/// </remarks>
public interface IServiceBusCredentialFactory
{
    /// <summary>
    /// Creates a <see cref="TokenCredential"/> instance for authenticating
    /// with Azure Service Bus.
    /// </summary>
    /// <returns>
    /// A configured <see cref="TokenCredential"/> implementation.
    /// </returns>
    TokenCredential CreateCredential();
}
