
# ConfigManagement.Shared.KeyVault

An opinionated, production-focused .NET package for interacting with **Azure Key Vault Secrets**
using **Azure.Identity**.

This package provides a clean abstraction over the Azure SDK with a strong emphasis on:

- Secure secret handling
- Explicit create vs update semantics
- Centralized authentication
- DI-first, testable design
- Consistency with other ConfigManagement packages

---

## Features

- ✔ Azure Key Vault Secrets support
- ✔ Azure.Identity authentication (no connection strings)
- ✔ Check if a secret exists
- ✔ Create secrets (create-only semantics)
- ✔ Update secrets (upsert semantics)
- ✔ Delete secrets (soft-delete aware)
- ✔ Centralized authentication via a credential factory
- ✔ Structured logging (no secret values logged)

---

## Installation

```bash
dotnet add package Azure.Security.KeyVault.Secrets
dotnet add package Azure.Identity
```

---

## Security Model

This package follows these security principles:

- Secrets are **never logged**
- Secrets are passed as `BinaryData`, not as raw strings
- Authentication uses Azure AD (RBAC-friendly)
- No connection strings or shared keys

> **Note:** Azure Key Vault enforces soft-delete by default.
> Deleting a secret does not immediately purge it.

---

## Authentication

Authentication is handled via a credential factory and supports:

- `DefaultAzureCredential`
- `ManagedIdentityCredential`
- `ClientSecretCredential`
- `AzureCliCredential`
- `VisualStudioCredential`

The authentication mechanism is configured once and reused across clients.

---

## Configuration

```json
{
  "KeyVault": {
    "VaultUri": "https://myvault.vault.azure.net",
    "AuthType": "Default",

    "TenantId": "",
    "ClientId": "",
    "ClientSecret": "",
    "ManagedIdentityClientId": ""
  }
}
```

---

## Public Interface

### IKeyVaultSecretClient

```csharp
public interface IKeyVaultSecretClient
{
    Task<bool> SecretExistsAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> CreateSecretAsync(
        string name,
        BinaryData value,
        CancellationToken cancellationToken = default);

    Task SetSecretAsync(
        string name,
        BinaryData value,
        CancellationToken cancellationToken = default);

    Task DeleteSecretAsync(
        string name,
        CancellationToken cancellationToken = default);
}
```

---

## Method Semantics

| Method | Behavior |
|------|---------|
| `SecretExistsAsync` | Checks whether a secret exists |
| `CreateSecretAsync` | Creates a secret only if it does not exist |
| `SetSecretAsync` | Creates or updates a secret |
| `DeleteSecretAsync` | Soft-deletes a secret |

---

## Dependency Injection Setup

### Register Authentication Factory

```csharp
services.AddSingleton<IKeyVaultCredentialFactory>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<KeyVaultCredentialFactory>>();

    var options = new KeyVaultAuthOptions
    {
        AuthType = Enum.Parse<KeyVaultAuthType>(
            configuration["KeyVault:AuthType"] ?? "Default"),

        TenantId = configuration["KeyVault:TenantId"],
        ClientId = configuration["KeyVault:ClientId"],
        ClientSecret = configuration["KeyVault:ClientSecret"],
        ManagedIdentityClientId =
            configuration["KeyVault:ManagedIdentityClientId"]
    };

    return new KeyVaultCredentialFactory(options, logger);
});
```

---

### Register Key Vault Secret Client

```csharp
services.AddSingleton<IKeyVaultSecretClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<KeyVaultSecretClient>>();
    var credentialFactory =
        sp.GetRequiredService<IKeyVaultCredentialFactory>();

    var options = new KeyVaultOptions
    {
        VaultUri = configuration["KeyVault:VaultUri"]
    };

    return new KeyVaultSecretClient(options, credentialFactory, logger);
});
```

---

## Usage Examples

### Check if a secret exists

```csharp
bool exists = await keyVaultClient.SecretExistsAsync("MySecret");
```

---

### Create a new secret (create-only)

```csharp
BinaryData secretValue =
    BinaryData.FromString("initial-secret-value");

bool created = await keyVaultClient.CreateSecretAsync(
    "MySecret",
    secretValue);
```

---

### Set (create or update) a secret

```csharp
BinaryData secretValue =
    BinaryData.FromString("updated-secret-value");

await keyVaultClient.SetSecretAsync(
    "MySecret",
    secretValue);
```

---

### Delete a secret

```csharp
await keyVaultClient.DeleteSecretAsync("MySecret");
```

---

## Recommended Practices

- Prefer `DefaultAzureCredential` in production
- Use Managed Identity wherever possible
- Scope RBAC permissions narrowly
- Separate read and write permissions if feasible
- Avoid frequent secret updates unless necessary

---

## Relationship to Other Packages

This package is designed to work alongside:

- `ConfigManagement.Shared.AppConfiguration`
- `ConfigManagement.Shared.ServiceBus`

All packages share:
- Azure.Identity authentication
- Factory-based credential creation
- DI-first architecture
- Consistent naming and semantics

---

## License

MIT (or your preferred license)
