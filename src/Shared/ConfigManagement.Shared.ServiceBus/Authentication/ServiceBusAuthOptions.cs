namespace ConfigManagement.Shared.ServiceBus.Authentication;

public sealed class ServiceBusAuthOptions
{
    public string FullyQualifiedNamespace { get; init; } = string.Empty;
    public ServiceBusAuthType AuthType { get; init; } = ServiceBusAuthType.Default;

    // Client secret auth
    public string? TenantId { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }

    // Managed identity
    public string? ManagedIdentityClientId { get; init; }
}
