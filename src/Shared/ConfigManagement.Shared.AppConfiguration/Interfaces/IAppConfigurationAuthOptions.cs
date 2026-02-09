using ConfigManagement.Shared.AppConfiguration.Enums;

namespace ConfigManagement.Shared.AppConfiguration.Interfaces;

public interface IAppConfigurationAuthOptions
{
    AuthType AuthType { get; }
    string? TenantId { get; }
    string? ClientId { get; }
    string? ClientSecret { get; }
    string? ManagedIdentityClientId { get; }
}
