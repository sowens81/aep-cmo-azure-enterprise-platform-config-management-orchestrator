
# ConfigManagement.Shared.ServiceBus

An opinionated, production-ready .NET package for publishing **events** and **results**
to **Azure Service Bus topics** using **Azure.Identity**.

This package provides a clean abstraction over the Azure Service Bus SDK with a focus on:

- Clear messaging semantics (events vs results)
- Centralized authentication via Azure.Identity
- Strongly-typed message envelopes
- Safe multi-environment publishing
- DI-first, testable design

---

## Features

- ✔ Azure Service Bus **Topic** publishing
- ✔ Azure.Identity authentication (no connection strings)
- ✔ Support for multiple auth types (Default, MI, Client Secret, CLI, VS)
- ✔ Strongly-typed `EventMessage<T>` and `ResultMessage<T>` models
- ✔ Clear separation of **Events** vs **Results**
- ✔ Environment-aware publishing
- ✔ Structured logging and error handling
- ✔ Async-safe publishers with proper disposal

---

## Installation

```bash
dotnet add package Azure.Messaging.ServiceBus
dotnet add package Azure.Identity
```

---

## Concepts

### Event vs Result

**Events**
- Represent facts that something happened
- Do not carry success/failure semantics
- Can be consumed by multiple downstream systems

**Results**
- Represent the outcome of an operation or workflow
- Include status and optional message
- Commonly correlated to commands or requests

This package enforces this distinction through separate publishers and message models.

---

## Message Models

### EventMessage

```csharp
public sealed class EventMessage<TPayload>
{
    public string EventType { get; init; } = default!;
    public string Source { get; init; } = default!;
    public string Environment { get; init; } = default!;
    public string CorrelationId { get; init; } = default!;
    public TPayload Payload { get; init; } = default!;
    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;
}
```

---

### ResultMessage

```csharp
public sealed class ResultMessage<TPayload>
{
    public string EventType { get; init; } = default!;
    public string Source { get; init; } = default!;
    public string Environment { get; init; } = default!;
    public string CorrelationId { get; init; } = default!;
    public string Status { get; init; } = default!;
    public string? Message { get; init; }
    public TPayload Payload { get; init; } = default!;
    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;
}
```

---

## Authentication

Authentication is handled centrally via a credential factory and supports:

- `DefaultAzureCredential`
- `ManagedIdentityCredential`
- `ClientSecretCredential`
- `AzureCliCredential`
- `VisualStudioCredential`

No connection strings are required.

---

## Configuration

```json
{
  "ServiceBus": {
    "FullyQualifiedNamespace": "mybus.servicebus.windows.net",
    "AuthType": "Default",

    "TenantId": "",
    "ClientId": "",
    "ClientSecret": "",
    "ManagedIdentityClientId": ""
  }
}
```

---

## Public Interfaces

### IEventPublisher

```csharp
public interface IEventPublisher
{
    Task PublishAsync<TPayload>(
        EventMessage<TPayload> message,
        CancellationToken cancellationToken);
}
```

---

### IResultPublisher

```csharp
public interface IResultPublisher
{
    Task PublishAsync<TPayload>(
        ResultMessage<TPayload> message,
        CancellationToken cancellationToken);
}
```

---

## Dependency Injection Setup

### Register Service Bus Credential Factory

```csharp
services.AddSingleton<IServiceBusCredentialFactory>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<ServiceBusCredentialFactory>>();

    var options = new ServiceBusAuthOptions
    {
        FullyQualifiedNamespace =
            configuration["ServiceBus:FullyQualifiedNamespace"],

        AuthType = Enum.Parse<ServiceBusAuthType>(
            configuration["ServiceBus:AuthType"] ?? "Default"),

        TenantId = configuration["ServiceBus:TenantId"],
        ClientId = configuration["ServiceBus:ClientId"],
        ClientSecret = configuration["ServiceBus:ClientSecret"],
        ManagedIdentityClientId =
            configuration["ServiceBus:ManagedIdentityClientId"]
    };

    return new ServiceBusCredentialFactory(options, logger);
});
```

---

### Register Event Topic Publisher

```csharp
services.AddSingleton<IEventPublisher>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<EventTopicPublisher>>();
    var factory = sp.GetRequiredService<ServiceBusCredentialFactory>();

    return new EventTopicPublisher(
        configuration["ServiceBus:EventTopic"]!,
        factory,
        logger);
});
```

---

### Register Result Topic Publisher

```csharp
services.AddSingleton<IResultPublisher>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<ResultTopicPublisher>>();
    var factory = sp.GetRequiredService<ServiceBusCredentialFactory>();

    return new ResultTopicPublisher(
        configuration["ServiceBus:ResultTopic"]!,
        factory,
        logger);
});
```

---

## Usage Examples

### Publish an Event

```csharp
await eventPublisher.PublishAsync(
    new EventMessage<MyPayload>
    {
        EventType = "ConfigUpdated",
        Source = "config-service",
        Environment = "prod",
        CorrelationId = correlationId,
        Payload = payload
    },
    cancellationToken);
```

---

### Publish a Result

```csharp
await resultPublisher.PublishAsync(
    new ResultMessage<MyPayload>
    {
        EventType = "ConfigUpdateCompleted",
        Source = "config-service",
        Environment = "prod",
        CorrelationId = correlationId,
        Status = "SUCCESS",
        Payload = payload
    },
    cancellationToken);
```

---

## Service Bus Mapping

| Field | Service Bus Property |
|-----|----------------------|
| `EventType` | `Subject` |
| `CorrelationId` | `CorrelationId` |
| `Source` | Application Property |
| `Environment` | Application Property |
| `Status` | Application Property (results only) |

---

## Recommended Practices

- Use separate topics for events and results
- Use `Environment` for cross-environment filtering
- Prefer `DefaultAzureCredential` in production
- Use Service Bus SQL filters on application properties
- Avoid logging payload contents

---

## Relationship to Other Packages

This package is designed to work alongside:

- `ConfigManagement.Shared.AppConfiguration`
- `ConfigManagement.Shared.KeyVault`

All packages share:
- Azure.Identity authentication
- Factory-based credential creation
- DI-first architecture
- Consistent naming and semantics

---

## License

MIT (or your preferred license)
