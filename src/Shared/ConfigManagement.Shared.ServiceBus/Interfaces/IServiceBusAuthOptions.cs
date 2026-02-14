using ConfigManagement.Shared.ServiceBus.Enums;

namespace ConfigManagement.Shared.ServiceBus.Interfaces;

/// <summary>
/// Defines authentication configuration required for connecting to Azure Service Bus.
/// </summary>
/// <remarks>
/// This interface abstracts the authentication settings used by the messaging
/// infrastructure. It supports multiple authentication mechanisms as defined
/// by <see cref="AuthType"/>.
///
/// Depending on the selected <see cref="AuthType"/>, only a subset of the
/// properties may be required:
/// <list type="bullet">
/// <item>
/// <description><b>Default</b> – Uses the default Azure credential chain.</description>
/// </item>
/// <item>
/// <description><b>Client Secret</b> – Requires <see cref="TenantId"/>, <see cref="ClientId"/>, and <see cref="ClientSecret"/>.</description>
/// </item>
/// <item>
/// <description><b>Managed Identity</b> – Optionally requires <see cref="ManagedIdentityClientId"/> for user-assigned identities.</description>
/// </item>
/// </list>
/// </remarks>
public interface IServiceBusAuthOptions
{
    /// <summary>
    /// Gets the authentication type used to acquire credentials.
    /// </summary>
    AuthType AuthType { get; }

    /// <summary>
    /// Gets the Azure Active Directory tenant identifier.
    /// </summary>
    /// <remarks>
    /// Required when using client secret–based authentication.
    /// </remarks>
    string? TenantId { get; }

    /// <summary>
    /// Gets the Azure Active Directory application (client) identifier.
    /// </summary>
    /// <remarks>
    /// Required when using client secret–based authentication.
    /// </remarks>
    string? ClientId { get; }

    /// <summary>
    /// Gets the Azure Active Directory application client secret.
    /// </summary>
    /// <remarks>
    /// Required when using client secret–based authentication.
    /// </remarks>
    string? ClientSecret { get; }

    /// <summary>
    /// Gets the client identifier of a user-assigned managed identity.
    /// </summary>
    /// <remarks>
    /// Optional when using managed identity authentication.
    /// Not required for system-assigned managed identities.
    /// </remarks>
    string? ManagedIdentityClientId { get; }
}
