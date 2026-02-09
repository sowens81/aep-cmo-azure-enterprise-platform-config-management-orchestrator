using ConfigManagement.Shared.AppConfiguration.Enums;
using ConfigManagement.Shared.AppConfiguration.Interfaces;

namespace ConfigManagement.Shared.AppConfiguration.Options;

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
