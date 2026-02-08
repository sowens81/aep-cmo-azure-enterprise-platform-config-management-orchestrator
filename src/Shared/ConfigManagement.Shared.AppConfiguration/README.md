
# ConfigManagement.Shared.AppConfiguration

An opinionated, production-ready .NET package for interacting with **Azure App Configuration**
using **Azure.Identity**.

This package provides a safe, explicit abstraction over the Azure SDK with a strong focus on:

- Clear intent (get vs add vs set)
- Secure integration with Azure Key Vault references
- Centralized authentication
- DI-first, testable design
- Multi-environment support via labels

---

## Features

- ✔ Azure App Configuration key-value management
- ✔ Azure.Identity authentication (no connection strings)
- ✔ Check if a configuration setting exists
- ✔ Create-only (add) semantics
- ✔ Create-or-update (set) semantics
- ✔ Key Vault reference helpers
- ✔ Delete configuration settings
- ✔ Centralized content type handling
- ✔ Structured logging

---

## Installation

```bash
dotnet add package Azure.Data.AppConfiguration
dotnet add package Azure.Identity
```

---

## Core Concepts

### Key + Label

In Azure App Configuration, a **key** combined with a **label** uniquely identifies a setting.

Examples:

- `MySetting` + `(null)`
- `MySetting` + `dev`
- `MySetting` + `prod`

These are **distinct entries**.
This package requires explicit label usage to avoid accidental overwrites.

---

## Content Types

This package defines known content types internally:

- `text/plain`
- `application/vnd.microsoft.appconfig.keyvaultref+json`

Consumers should not hard-code these values.

---

## Authentication

Authentication is handled via a credential factory and supports:

- `DefaultAzureCredential`
- `ManagedIdentityCredential`
- `ClientSecretCredential`
- `AzureCliCredential`
- `VisualStudioCredential`

No connection strings are required.

---

## Public Interface

### IAppConfigurationClient

```csharp
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
        string contentType = "text/plain",
        CancellationToken cancellationToken = default);

    Task<bool> AddKeyVaultReferenceConfigurationSettingAsync(
        string key,
        Uri secretUri,
        string? label = null,
        string contentType = "application/vnd.microsoft.appconfig.keyvaultref+json",
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
```

---

## Semantics

| Method | Behavior |
|------|---------|
| `GetConfigurationSettingAsync` | Checks existence only |
| `AddConfigurationSettingAsync` | Creates key only if it does not exist |
| `AddKeyVaultReferenceConfigurationSettingAsync` | Creates Key Vault reference only if it does not exist |
| `SetConfigurationSettingAsync` | Creates or updates a key |
| `DeleteConfigurationSettingAsync` | Deletes a key (idempotent) |

---

## Configuration

```json
{
  "AppConfiguration": {
    "Endpoint": "https://myappconfig.azconfig.io",
    "AuthType": "Default",

    "TenantId": "",
    "ClientId": "",
    "ClientSecret": "",
    "ManagedIdentityClientId": ""
  }
}
```

---

## Dependency Injection Setup

### Register Authentication Factory

```csharp
services.AddSingleton<IAppConfigurationCredentialFactory>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<AppConfigurationCredentialFactory>>();

    var authOptions = new AppConfigurationAuthOptions
    {
        AuthType = Enum.Parse<AppConfigurationAuthType>(
            configuration["AppConfiguration:AuthType"] ?? "Default"),

        TenantId = configuration["AppConfiguration:TenantId"],
        ClientId = configuration["AppConfiguration:ClientId"],
        ClientSecret = configuration["AppConfiguration:ClientSecret"],
        ManagedIdentityClientId =
            configuration["AppConfiguration:ManagedIdentityClientId"]
    };

    return new AppConfigurationCredentialFactory(authOptions, logger);
});
```

---

### Register App Configuration Client

```csharp
services.AddSingleton<IAppConfigurationClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<AppConfigurationClient>>();
    var credentialFactory =
        sp.GetRequiredService<IAppConfigurationCredentialFactory>();

    var options = new AppConfigurationOptions
    {
        Endpoint = configuration["AppConfiguration:Endpoint"]
    };

    return new AppConfigurationClient(options, credentialFactory, logger);
});
```

---

## Usage Examples

### Check if a key exists

```csharp
bool exists = await client.GetConfigurationSettingAsync(
    "MySetting",
    "prod");
```

---

### Add a key (create only)

```csharp
bool created = await client.AddConfigurationSettingAsync(
    "MySetting",
    "initial-value",
    "prod");
```

---

### Add a Key Vault reference

```csharp
bool created = await client.AddKeyVaultReferenceConfigurationSettingAsync(
    "MySecretSetting",
    new Uri("https://myvault.vault.azure.net/secrets/MySecret"),
    "prod");
```

---

### Set (create or update) a key

```csharp
await client.SetConfigurationSettingAsync(
    "MySetting",
    "updated-value",
    "prod");
```

---

### Delete a key

```csharp
await client.DeleteConfigurationSettingAsync(
    "MySetting",
    "prod");
```

---

## Recommended Practices

- Use labels to represent environments
- Prefer `DefaultAzureCredential` in production
- Avoid wildcard deletes
- Log all destructive operations
- Use Key Vault references for secrets

---

## Changelog

See `CHANGELOG.md` for version history.

---

## License

MIT (or your preferred license)
