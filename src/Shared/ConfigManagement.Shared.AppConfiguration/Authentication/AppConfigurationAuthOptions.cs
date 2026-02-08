namespace ConfigManagement.Shared.AppConfiguration.Authentication;

public sealed class AppConfigurationAuthOptions
{
    public AppConfigurationAuthType AuthType { get; init; } =
        AppConfigurationAuthType.Default;

    // Client secret auth
    public string? TenantId { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }

    // Managed identity
    public string? ManagedIdentityClientId { get; init; }
}
