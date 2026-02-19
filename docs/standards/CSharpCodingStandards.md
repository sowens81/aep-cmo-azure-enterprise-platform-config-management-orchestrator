# C# Coding Standards

**Target Runtime:** .NET 8+ (Forward compatible with .NET 10)\
**Language Version:** C# 14 (Latest stable or preview where required)\
**Nullable Reference Types:** Enabled\
**Implicit Usings:** Enabled\
**Treat Warnings As Errors:** Enabled

------------------------------------------------------------------------

# 1. Core Principles

-   Prefer clarity over cleverness
-   Favor immutability
-   Minimize boilerplate using modern C# features
-   Use explicit intent-driven design
-   Follow Clean Architecture principles
-   Avoid unnecessary abstraction

------------------------------------------------------------------------

# 2. Type Guidelines

## Use `record` for:

-   DTOs
-   Value objects
-   API contracts
-   Messaging models
-   Configuration objects

``` csharp
public record CustomerDto(int Id, string Name, string Email);
```

Records provide: - Built-in immutability - Value-based equality -
Auto-generated `Equals` and `GetHashCode`

------------------------------------------------------------------------

## Use `class` for:

-   Entities
-   Services
-   Mutable domain objects

------------------------------------------------------------------------

## Use `interface` for:

-   Abstractions
-   Service contracts
-   Dependency injection boundaries

------------------------------------------------------------------------

# 3. Primary Constructors (C# 12+)

Use primary constructors to reduce constructor boilerplate.

### Before

``` csharp
public class OrderService
{
    private readonly IRepository _repo;

    public OrderService(IRepository repo)
    {
        _repo = repo;
    }
}
```

### After

``` csharp
public class OrderService(IRepository repo)
{
    private readonly IRepository _repo = repo;
}
```

Use when dependencies are clearly defined at construction time.

------------------------------------------------------------------------

# 4. Immutability Standards

## Use `init` accessors instead of backing fields

### Before

``` csharp
private string _name;
public string Name
{
    get => _name;
    set => _name = value ?? throw new ArgumentNullException();
}
```

### After

``` csharp
public string Name { get; init; } = string.Empty;
```

------------------------------------------------------------------------

## Use `with` expressions instead of manual mapping

### Before

``` csharp
var updated = new CustomerDto(old.Id, "New Name", old.Email);
```

### After

``` csharp
var updated = old with { Name = "New Name" };
```

------------------------------------------------------------------------

# 5. Modern Syntax Usage

## Use File-Scoped Namespaces

``` csharp
namespace MyApp.Services;
```

------------------------------------------------------------------------

## Use Target-Typed `new`

``` csharp
Customer customer = new(1, "Shoeb");
```

Avoid factory classes that add no behavior.

------------------------------------------------------------------------

## Use Collection Expressions (C# 12+)

### Before

``` csharp
var list = new List<int> { 1, 2, 3 };
```

### After

``` csharp
List<int> list = [1, 2, 3];
```

------------------------------------------------------------------------

## Use Named Tuples or Records Instead of Helper Classes

### Lightweight Case

``` csharp
(bool Success, string Error) result = GetResult();
```

Use a `record` when the concept deserves a name.

------------------------------------------------------------------------

# 6. Argument Validation

## Do NOT use verbose null-check guards

### Before

``` csharp
if (customer == null)
    throw new ArgumentNullException(nameof(customer));
```

### After

``` csharp
ArgumentNullException.ThrowIfNull(customer);
```

------------------------------------------------------------------------

# 7. Naming Conventions

  Element          Convention    Example
  ---------------- ------------- ----------------
  Classes          PascalCase    OrderService
  Interfaces       IPascalCase   IRepository
  Methods          PascalCase    CalculateTotal
  Private Fields   \_camelCase   \_logger
  Parameters       camelCase     orderId
  Constants        PascalCase    MaxRetryCount

------------------------------------------------------------------------

# 8. Async & Concurrency

-   All I/O must be async
-   Never use `.Result` or `.Wait()`
-   Async methods must end with `Async`
-   Avoid `async void` (except event handlers)
-   Use `ValueTask` only when performance-critical

------------------------------------------------------------------------

# 9. Dependency Injection

-   Use constructor injection only
-   Avoid service locator pattern
-   Avoid static dependencies
-   Prefer scoped services
-   Singletons must be thread-safe

------------------------------------------------------------------------

# 10. Logging Standards

-   Use structured logging
-   Never log sensitive data
-   Use message templates (NOT string interpolation)

``` csharp
_logger.LogInformation("User {UserId} created", user.Id);
```

------------------------------------------------------------------------

# 11. Nullable Reference Types

-   Nullable must remain enabled
-   Do not suppress warnings without justification
-   Return empty collections instead of null
-   Use explicit nullability annotations

------------------------------------------------------------------------

# 12. Clean Code Limits

-   Max method length: 40 lines
-   Max class length: 300 lines
-   Cyclomatic complexity \< 10
-   One public class per file
-   No region blocks
-   No commented-out code

------------------------------------------------------------------------

# 13. Security Standards

-   Validate all input
-   Never store secrets in source code
-   Use HTTPS redirection
-   Use authorization policies
-   Sanitize logging output

------------------------------------------------------------------------

# 14. Documentation Requirements

-   All public members must include XML documentation
-   Use `<inheritdoc />` where appropriate
-   Maintain README setup instructions

------------------------------------------------------------------------

# 15. Prohibited Practices

-   No static mutable state
-   No blocking calls in ASP.NET
-   No reflection unless justified
-   No magic strings (use constants)
-   No manual equality overrides for value objects

------------------------------------------------------------------------

# 16. Recommended Project Settings

``` xml
<TargetFramework>net8.0</TargetFramework>
<LangVersion>preview</LangVersion>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
<AnalysisLevel>latest</AnalysisLevel>
<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
```

------------------------------------------------------------------------

# 17. Architectural Guidance

Recommended folder structure:

    src/
      Application/
      Domain/
      Infrastructure/
      Web/
    tests/

-   Domain has no external dependencies
-   Infrastructure depends on Application + Domain
-   Web depends on Application only

------------------------------------------------------------------------

# Summary

This standard enforces: - Modern C# usage - Reduced boilerplate - Strong
immutability - Explicit architecture boundaries - Production-grade
reliability

Designed for high-quality enterprise .NET development and AI-assisted
coding environments.
