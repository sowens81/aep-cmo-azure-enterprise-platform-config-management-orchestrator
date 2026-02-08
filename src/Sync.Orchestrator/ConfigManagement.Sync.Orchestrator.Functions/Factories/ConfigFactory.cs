using ConfigManagement.Shared.AppConfiguration.Authentication;
using ConfigManagement.Shared.KeyVault.Authentication;
using ConfigManagement.Shared.ServiceBus.Authentication;

namespace ConfigurationSyncOrchestrator.Domain.Factories;

public sealed class ConfigFactory
{
    public string Organisation { get; }
    public string Region { get; }
    public string EnvironmentTier { get; }
    public string EnvironmentName { get; }
    public string ServiceName { get; }

    public string ServiceBusNamespaceEndpoint { get; }
    public ServiceBusAuthOptions ServiceBusOptions { get; }
    public string ServiceBusEventTopic { get; }
    public string ServiceBusEventTopicSubscription { get; }
    public string ServiceBusEventResultTopic { get; }

    public string AppConfigEndpoint { get; }

    public AppConfigurationAuthOptions AppConfigOptions { get; }
    public KeyVaultAuthOptions KeyVaultOptions { get; }

    public string LocalKeyVaultUri { get; }
    public string HubKeyVaultUri { get; }

    public string StorageAccountTableUri { get; }
    public string StorageAccountBlobUri { get; }
    public string IdempotencyTableName { get; }

    public ConfigFactory()
    {
        Organisation = GetRequired("ORGANISATION");
        Region = GetRequired("REGION");
        EnvironmentTier = GetRequired("ENVIRONMENT_TIER");
        EnvironmentName = GetRequired("ENVIRONMENT_NAME");
        ServiceName = GetRequired("SERVICE_NAME");

        ServiceBusNamespaceEndpoint = GetRequired("ServiceBus__FullyQualifiedNamespace");
        ServiceBusOptions = GetServiceBusSection("ServiceBus");
        ServiceBusEventTopic = GetRequired("SBUS_EVENT_TOPIC");
        ServiceBusEventTopicSubscription = GetRequired("SBUS_EVENT_TOPIC_SUBSCRIPTION");
        ServiceBusEventResultTopic = GetRequired("SBUS_EVENT_RESULT_TOPIC");


        AppConfigEndpoint = GetRequired("AppConfiguration__Endpoint");
        AppConfigOptions = GetAppConfigurationSection("AppConfiguration");

        KeyVaultOptions = GetKeyVaultSection("KeyVault");

        LocalKeyVaultUri = GetRequired("KeyVault__LocalVaultUri");
        HubKeyVaultUri = GetRequired("KeyVault__HubKeyVaultUri");

        StorageAccountTableUri = GetRequired("STORAGE_ACCOUNT_TABLE_URI");
        StorageAccountBlobUri = GetRequired("STORAGE_ACCOUNT_BLOB_URI");
        IdempotencyTableName = GetRequired("IDEMPOTENCY_TABLE_NAME");
    }

    private static string GetRequired(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(
                $"Required environment variable '{name}' is not set.");

        return value;
    }

    private static AppConfigurationAuthOptions GetAppConfigurationSection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Section name must be provided", nameof(name));

        string GetRequiredFromSection(string key)
            => GetRequired($"{name}__{key}");

        return new AppConfigurationAuthOptions
        {
            AuthType = Enum.Parse<AppConfigurationAuthType>(
                GetRequiredFromSection(nameof(AppConfigurationAuthOptions.AuthType)),
                ignoreCase: true),
            TenantId = GetRequiredFromSection(nameof(AppConfigurationAuthOptions.TenantId)),
            ClientId = GetRequiredFromSection(nameof(AppConfigurationAuthOptions.ClientId)),
            ClientSecret = GetRequiredFromSection(nameof(AppConfigurationAuthOptions.ClientSecret)),
            ManagedIdentityClientId = GetRequiredFromSection(nameof(AppConfigurationAuthOptions.ManagedIdentityClientId))
        };
    }

    private static ServiceBusAuthOptions GetServiceBusSection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Section name must be provided", nameof(name));

        string GetRequiredFromSection(string key)
            => GetRequired($"{name}__{key}");

        return new ServiceBusAuthOptions
        {
            FullyQualifiedNamespace = GetRequiredFromSection(nameof(ServiceBusAuthOptions.FullyQualifiedNamespace)),
            AuthType = Enum.Parse<ServiceBusAuthType>(
                GetRequiredFromSection(nameof(ServiceBusAuthOptions.AuthType)),
                ignoreCase: true),
            TenantId = GetRequiredFromSection(nameof(ServiceBusAuthOptions.TenantId)),
            ClientId = GetRequiredFromSection(nameof(ServiceBusAuthOptions.ClientId)),
            ClientSecret = GetRequiredFromSection(nameof(ServiceBusAuthOptions.ClientSecret)),
            ManagedIdentityClientId = GetRequiredFromSection(nameof(ServiceBusAuthOptions.ManagedIdentityClientId))
        };
    }

    private static KeyVaultAuthOptions GetKeyVaultSection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Section name must be provided", nameof(name));

        string GetRequiredFromSection(string key)
            => GetRequired($"{name}__{key}");

        return new KeyVaultAuthOptions
        {
            AuthType = Enum.Parse<KeyVaultAuthType>(
                GetRequiredFromSection(nameof(KeyVaultAuthOptions.AuthType)),
                ignoreCase: true),
            TenantId = GetRequiredFromSection(nameof(KeyVaultAuthOptions.TenantId)),
            ClientId = GetRequiredFromSection(nameof(KeyVaultAuthOptions.ClientId)),
            ClientSecret = GetRequiredFromSection(nameof(KeyVaultAuthOptions.ClientSecret)),
            ManagedIdentityClientId = GetRequiredFromSection(nameof(KeyVaultAuthOptions.ManagedIdentityClientId))
        };
    }
}