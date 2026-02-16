using Aep.Cmo.Shared.AppConfiguration.Interfaces;

namespace Aep.Cmo.Shared.AppConfiguration.Options;

public sealed class AppConfigurationOptions : IAppConfigurationOptions
{
    public string Endpoint { get; init; } = default!;
}
