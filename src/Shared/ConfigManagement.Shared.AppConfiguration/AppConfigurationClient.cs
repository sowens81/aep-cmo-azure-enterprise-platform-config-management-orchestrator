using Azure;
using Azure.Data.AppConfiguration;
using ConfigManagement.Shared.AppConfiguration.Constants;
using ConfigManagement.Shared.AppConfiguration.Interfaces;
using ConfigManagement.Shared.AppConfiguration.OpenTelemetry;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ConfigManagement.Shared.AppConfiguration;

/// <summary>
/// Provides operations for interacting with Azure App Configuration.
/// </summary>
/// <remarks>
/// Each operation creates an OpenTelemetry span (<see cref="ActivityKind.Client"/>)
/// to enable distributed tracing and dependency visibility.
/// </remarks>
public sealed class AppConfigurationClient : IAppConfigurationClient
{
    private readonly ConfigurationClient _client;
    private readonly ILogger<AppConfigurationClient> _logger;

    public AppConfigurationClient(
        IAppConfigurationOptions options,
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
        using var activity = Telemetry.Source.StartActivity(
            "AppConfiguration Get",
            ActivityKind.Client);

        activity?.SetTag("rpc.system", "azure.appconfiguration");
        activity?.SetTag("rpc.method", "GetConfigurationSetting");
        activity?.SetTag("appconfig.key", key);
        activity?.SetTag("appconfig.label", label ?? "<null>");

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
            activity?.SetStatus(ActivityStatusCode.Error, "Not Found");
            return false;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Failed to get App Configuration key {Key}, Label={Label}",
                key,
                label ?? "<null>");

            throw;
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
        using var activity = Telemetry.Source.StartActivity(
            "AppConfiguration Add",
            ActivityKind.Client);

        activity?.SetTag("rpc.system", "azure.appconfiguration");
        activity?.SetTag("rpc.method", "AddConfigurationSetting");
        activity?.SetTag("appconfig.key", key);
        activity?.SetTag("appconfig.label", label ?? "<null>");

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
            activity?.SetStatus(ActivityStatusCode.Error, "Already Exists");

            _logger.LogInformation(
                "App Configuration key already exists {Key}, Label={Label}",
                key,
                label ?? "<null>");

            return false;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Failed to add App Configuration key {Key}, Label={Label}",
                key,
                label ?? "<null>");

            throw;
        }
    }

    // ------------------------------------------------------------
    // ADD KEY VAULT REFERENCE (CREATE ONLY)
    // ------------------------------------------------------------

    public async Task<bool> AddKeyVaultReferenceConfigurationSettingAsync(
        string key,
        Uri secretUri,
        string? label = null,
        string contentType = ContentTypes.KeyVaultReference,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Source.StartActivity(
            "AppConfiguration Add KeyVaultReference",
            ActivityKind.Client);

        activity?.SetTag("rpc.system", "azure.appconfiguration");
        activity?.SetTag("rpc.method", "AddKeyVaultReferenceConfigurationSetting");
        activity?.SetTag("appconfig.key", key);
        activity?.SetTag("appconfig.label", label ?? "<null>");
        activity?.SetTag("appconfig.kv.reference", secretUri.ToString());

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
            activity?.SetStatus(ActivityStatusCode.Error, "Already Exists");

            _logger.LogInformation(
                "Key Vault reference already exists {Key}, Label={Label}",
                key,
                label ?? "<null>");

            return false;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Failed to add Key Vault reference {Key}, Label={Label}",
                key,
                label ?? "<null>");

            throw;
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
        using var activity = Telemetry.Source.StartActivity(
            "AppConfiguration Set",
            ActivityKind.Client);

        activity?.SetTag("rpc.system", "azure.appconfiguration");
        activity?.SetTag("rpc.method", "SetConfigurationSetting");
        activity?.SetTag("appconfig.key", key);
        activity?.SetTag("appconfig.label", label ?? "<null>");

        try
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
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Failed to set App Configuration key {Key}, Label={Label}",
                key,
                label ?? "<null>");

            throw;
        }
    }

    // ------------------------------------------------------------
    // DELETE
    // ------------------------------------------------------------

    public async Task DeleteConfigurationSettingAsync(
        string key,
        string? label = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Source.StartActivity(
            "AppConfiguration Delete",
            ActivityKind.Client);

        activity?.SetTag("rpc.system", "azure.appconfiguration");
        activity?.SetTag("rpc.method", "DeleteConfigurationSetting");
        activity?.SetTag("appconfig.key", key);
        activity?.SetTag("appconfig.label", label ?? "<null>");

        try
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
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Failed to delete App Configuration key {Key}, Label={Label}",
                key,
                label ?? "<null>");

            throw;
        }
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
