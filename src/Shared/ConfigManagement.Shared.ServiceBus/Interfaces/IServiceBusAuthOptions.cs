using ConfigManagement.Shared.ServiceBus.Enums;

namespace ConfigManagement.Shared.ServiceBus.Interfaces;

public interface IServiceBusAuthOptions
{
    AuthType AuthType { get; }
    string? TenantId { get; }
    string? ClientId { get; }
    string? ClientSecret { get; }
    string? ManagedIdentityClientId { get; }
}
