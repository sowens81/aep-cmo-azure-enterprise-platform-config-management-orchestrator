using ConfigManagement.Shared.ServiceBus.Enums;
using ConfigManagement.Shared.ServiceBus.Interfaces;

namespace ConfigManagement.Shared.ServiceBus.Options;

public sealed class ServiceBusAuthOptions : IServiceBusAuthOptions
{
    public AuthType AuthType { get; init; } =
        AuthType.Default;

    // Client secret auth
    public string? TenantId { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }

    // Managed identity
    public string? ManagedIdentityClientId { get; init; }
}
