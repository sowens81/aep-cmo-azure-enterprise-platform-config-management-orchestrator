namespace ConfigManagement.Shared.AppConfiguration;

public interface IAppConfigurationClient
{
    Task<bool> GetConfigurationSettingAsync(
        string key,
        string? label = null,
        CancellationToken cancellationToken = default);

    Task<bool> AddConfigurationSettingAsync(
        string key,
        string value,
        string? label = null,
        string contentType = AppConfigurationContentTypes.PlainText,
        CancellationToken cancellationToken = default);

    Task<bool> AddKeyVaultReferenceConfigurationSettingAsync(
        string key,
        Uri secretUri,
        string? label = null,
        string contentType = AppConfigurationContentTypes.KeyVaultReference,
        CancellationToken cancellationToken = default);

    Task SetConfigurationSettingAsync(
        string key,
        string value,
        string? label = null,
        CancellationToken cancellationToken = default);

    Task DeleteConfigurationSettingAsync(
        string key,
        string? label = null,
        CancellationToken cancellationToken = default);
}
