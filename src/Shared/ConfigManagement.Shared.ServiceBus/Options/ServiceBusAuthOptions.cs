using ConfigManagement.Shared.ServiceBus.Enums;
using ConfigManagement.Shared.ServiceBus.Interfaces;

namespace ConfigManagement.Shared.ServiceBus.Options;

/// <summary>
/// Represents authentication configuration options for connecting to Azure Service Bus.
/// </summary>
/// <remarks>
/// This options class supports multiple authentication mechanisms defined by <see cref="AuthType"/>:
/// <list type="bullet">
/// <item>
/// <description><b>Default</b> – Uses the default Azure credential chain (e.g., environment, managed identity, Visual Studio, etc.).</description>
/// </item>
/// <item>
/// <description><b>Client Secret</b> – Uses Azure AD application credentials (TenantId, ClientId, ClientSecret).</description>
/// </item>
/// <item>
/// <description><b>Managed Identity</b> – Uses a system-assigned or user-assigned managed identity.</description>
/// </item>
/// </list>
/// 
/// Only the properties relevant to the selected <see cref="AuthType"/> need to be populated.
/// All properties use <c>init</c> accessors to ensure immutability after configuration binding.
/// </remarks>
public sealed class ServiceBusAuthOptions : IServiceBusAuthOptions
{
    /// <summary>
    /// Gets the authentication type used to connect to Azure Service Bus.
    /// Defaults to <see cref="AuthType.Default"/>.
    /// </summary>
    public AuthType AuthType { get; init; } =
        AuthType.Default;

    // Client secret auth

    /// <summary>
    /// Gets the Azure Active Directory tenant identifier.
    /// Required when <see cref="AuthType"/> is set to a client secret–based authentication type.
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets the Azure Active Directory application (client) identifier.
    /// Required when <see cref="AuthType"/> is set to a client secret–based authentication type.
    /// </summary>
    public string? ClientId { get; init; }

    /// <summary>
    /// Gets the Azure Active Directory application client secret.
    /// Required when <see cref="AuthType"/> is set to a client secret–based authentication type.
    /// </summary>
    public string? ClientSecret { get; init; }

    // Managed identity

    /// <summary>
    /// Gets the client identifier of a user-assigned managed identity.
    /// </summary>
    /// <remarks>
    /// This property is only applicable when <see cref="AuthType"/> is set to a managed identity–based authentication type.
    /// For system-assigned managed identities, this value can be <c>null</c>.
    /// </remarks>
    public string? ManagedIdentityClientId { get; init; }
}
