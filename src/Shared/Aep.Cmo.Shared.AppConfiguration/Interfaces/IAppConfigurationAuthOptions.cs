using Aep.Cmo.Shared.AppConfiguration.Enums;

namespace Aep.Cmo.Shared.AppConfiguration.Interfaces;

public interface IAppConfigurationAuthOptions
{
    AuthType AuthType { get; }
    string? TenantId { get; }
    string? ClientId { get; }
    string? ClientSecret { get; }
    string? ManagedIdentityClientId { get; }
}
