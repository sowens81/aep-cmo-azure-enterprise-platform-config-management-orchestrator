using Azure;
using Azure.Data.AppConfiguration;
using ConfigManagement.Shared.AppConfiguration.Constants;
using ConfigManagement.Shared.AppConfiguration.Interfaces;
using ConfigManagement.Shared.AppConfiguration.Options;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Shared.AppConfiguration;

public sealed class AppConfigurationClient : IAppConfigurationClient
{
    private readonly ConfigurationClient _client;
    private readonly ILogger<AppConfigurationClient> _logger;

    public AppConfigurationClient(
        AppConfigurationOptions options,
        IAppConfigurationCredentialFactory credentialFactory,
        ILogger<AppConfigurationClient> logger)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint))
            throw new ArgumentException("Endpoint is required.", nameof(options));

        _client = new ConfigurationClient(
            new Uri(options.Endpoint),
            credentialFactory.CreateCredential());

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ------------------------------------------------------------
    // GET
    // ------------------------------------------------------------

    public async Task<bool> GetConfigurationSettingAsync(
        string key,
        string? label = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.GetConfigurationSettingAsync(
                key,
                label,
                cancellationToken);

            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    // ------------------------------------------------------------
    // ADD (CREATE ONLY)
    // ------------------------------------------------------------

    public async Task<bool> AddConfigurationSettingAsync(
        string key,
        string value,
        string? label = null,
        string contentType = ContentTypes.PlainText,
        CancellationToken cancellationToken = default)
    {
        var setting = new ConfigurationSetting(key, value, label)
        {
            ContentType = contentType,
        };

        try
        {
            await _client.AddConfigurationSettingAsync(
                setting,
                cancellationToken);

            _logger.LogInformation(
                "Added App Configuration key {Key}, Label={Label}",
                key,
                label ?? "<null>");

            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 412)
        {
            // Already exists
            _logger.LogInformation(
                "App Configuration key already exists {Key}, Label={Label}",
                key,
                label ?? "<null>");

            return false;
        }
    }

    public async Task<bool> AddKeyVaultReferenceConfigurationSettingAsync(
        string key,
        Uri secretUri,
        string? label = null,
        string contentType = ContentTypes.KeyVaultReference,
        CancellationToken cancellationToken = default)
    {
        var setting = CreateKeyVaultReferenceSetting(
            key,
            secretUri,
            label,
            contentType);

        try
        {
            await _client.AddConfigurationSettingAsync(
                setting,
                cancellationToken);

            _logger.LogInformation(
                "Added Key Vault reference {Key}, Label={Label}",
                key,
                label ?? "<null>");

            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 412)
        {
            return false;
        }
    }

    // ------------------------------------------------------------
    // SET (UPSERT)
    // ------------------------------------------------------------

    public async Task SetConfigurationSettingAsync(
        string key,
        string value,
        string? label = null,
        CancellationToken cancellationToken = default)
    {

        await _client.SetConfigurationSettingAsync(
            key,
            value,
            label,
            cancellationToken);

        _logger.LogInformation(
            "Set App Configuration key {Key}, Label={Label}",
            key,
            label ?? "<null>");
    }

    // ------------------------------------------------------------
    // DELETE
    // ------------------------------------------------------------

    public async Task DeleteConfigurationSettingAsync(
        string key,
        string? label = null,
        CancellationToken cancellationToken = default)
    {
        await _client.DeleteConfigurationSettingAsync(
            key,
            label,
            cancellationToken);

        _logger.LogInformation(
            "Deleted App Configuration key {Key}, Label={Label}",
            key,
            label ?? "<null>");
    }

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------

    private static ConfigurationSetting CreateKeyVaultReferenceSetting(
        string key,
        Uri secretUri,
        string? label,
        string contentType)
    {
        if (secretUri is null)
            throw new ArgumentNullException(nameof(secretUri));

        return new ConfigurationSetting(key, string.Empty, label)
        {
            ContentType = contentType,
            Value = $@"{{""uri"":""{secretUri}""}}"
        };
    }
}
