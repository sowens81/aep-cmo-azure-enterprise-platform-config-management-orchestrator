namespace ConfigManagement.Shared.ServiceBus.Enums;

/// <summary>
/// Specifies the authentication mechanism used to acquire credentials
/// for connecting to Azure Service Bus.
/// </summary>
/// <remarks>
/// This enumeration determines how a <c>TokenCredential</c> should be created
/// by the credential factory implementation.
///
/// The selected value controls which configuration properties are required
/// and which Azure identity provider is used.
/// </remarks>
public enum AuthType
{
    /// <summary>
    /// Uses the default Azure credential chain.
    /// </summary>
    /// <remarks>
    /// Typically maps to <c>DefaultAzureCredential</c>, which attempts multiple
    /// authentication mechanisms in order (environment variables, managed identity,
    /// Visual Studio, Azure CLI, etc.).
    /// Recommended for most scenarios.
    /// </remarks>
    Default,

    /// <summary>
    /// Uses Azure Managed Identity for authentication.
    /// </summary>
    /// <remarks>
    /// Supports both system-assigned and user-assigned managed identities.
    /// If using a user-assigned identity, a client identifier may be required.
    /// </remarks>
    ManagedIdentity,

    /// <summary>
    /// Uses Azure Active Directory application credentials (client secret).
    /// </summary>
    /// <remarks>
    /// Requires TenantId, ClientId, and ClientSecret to be configured.
    /// Suitable for service-to-service authentication scenarios.
    /// </remarks>
    ClientSecret,

    /// <summary>
    /// Uses the authenticated Azure CLI session.
    /// </summary>
    /// <remarks>
    /// Intended primarily for local development environments
    /// where the user is authenticated via <c>az login</c>.
    /// </remarks>
    AzureCli,

    /// <summary>
    /// Uses the authenticated Visual Studio session.
    /// </summary>
    /// <remarks>
    /// Intended for local development when signed in to Visual Studio
    /// with an Azure account.
    /// </remarks>
    VisualStudio
}
