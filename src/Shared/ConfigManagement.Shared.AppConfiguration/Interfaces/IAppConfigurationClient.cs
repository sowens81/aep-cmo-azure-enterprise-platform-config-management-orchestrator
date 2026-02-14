using ConfigManagement.Shared.AppConfiguration.Constants;

namespace ConfigManagement.Shared.AppConfiguration.Interfaces;

/// <summary>
/// Defines operations for interacting with Azure App Configuration.
/// </summary>
/// <remarks>
/// This abstraction encapsulates common configuration management operations,
/// including retrieving, adding, updating, and deleting configuration settings,
/// as well as creating Key Vault reference entries.
///
/// Implementations of this interface are responsible for:
/// <list type="bullet">
/// <item><description>Communicating with Azure App Configuration.</description></item>
/// <item><description>Handling serialization and content types.</description></item>
/// <item><description>Applying appropriate error handling and retry logic.</description></item>
/// </list>
/// </remarks>
public interface IAppConfigurationClient
{
    /// <summary>
    /// Retrieves a configuration setting by key and optional label.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="label">
    /// The optional label used to distinguish environment- or version-specific values.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// <c>true</c> if the configuration setting exists; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> GetConfigurationSettingAsync(
        string key,
        string? label = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new configuration setting.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The configuration value.</param>
    /// <param name="label">
    /// The optional label used to distinguish environment- or version-specific values.
    /// </param>
    /// <param name="contentType">
    /// The content type of the configuration value.
    /// Defaults to <see cref="ContentTypes.PlainText"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// <c>true</c> if the setting was successfully added; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> AddConfigurationSettingAsync(
        string key,
        string value,
        string? label = null,
        string contentType = ContentTypes.PlainText,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new configuration setting that references an Azure Key Vault secret.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="secretUri">
    /// The URI of the Key Vault secret to reference.
    /// </param>
    /// <param name="label">
    /// The optional label used to distinguish environment- or version-specific values.
    /// </param>
    /// <param name="contentType">
    /// The content type for Key Vault references.
    /// Defaults to <see cref="ContentTypes.KeyVaultReference"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// <c>true</c> if the Key Vault reference setting was successfully added; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> AddKeyVaultReferenceConfigurationSettingAsync(
        string key,
        Uri secretUri,
        string? label = null,
        string contentType = ContentTypes.KeyVaultReference,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing configuration setting or creates it if it does not exist.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The configuration value.</param>
    /// <param name="label">
    /// The optional label used to distinguish environment- or version-specific values.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the operation to complete.
    /// </param>
    Task SetConfigurationSettingAsync(
        string key,
        string value,
        string? label = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a configuration setting by key and optional label.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="label">
    /// The optional label used to distinguish environment- or version-specific values.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the operation to complete.
    /// </param>
    Task DeleteConfigurationSettingAsync(
        string key,
        string? label = null,
        CancellationToken cancellationToken = default);
}
