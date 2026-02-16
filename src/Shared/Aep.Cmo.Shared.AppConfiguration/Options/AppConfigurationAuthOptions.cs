using Aep.Cmo.Shared.AppConfiguration.Enums;
using Aep.Cmo.Shared.AppConfiguration.Interfaces;

namespace Aep.Cmo.Shared.AppConfiguration.Options;

public sealed class AppConfigurationAuthOptions : IAppConfigurationAuthOptions
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
