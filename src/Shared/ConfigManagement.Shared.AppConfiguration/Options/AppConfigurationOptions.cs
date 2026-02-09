using ConfigManagement.Shared.AppConfiguration.Interfaces;

namespace ConfigManagement.Shared.AppConfiguration.Options;

public sealed class AppConfigurationOptions : IAppConfigurationOptions
{
    public string Endpoint { get; init; } = default!;
}
