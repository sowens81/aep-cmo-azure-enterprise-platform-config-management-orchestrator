# Azure Enterprise Platform - Config Management Orchestrator (AEP-CMO)

Proof-of-concept (PoC) implementation of hub → spoke configuration and secret synchronisation for an Azure Enterprise Platform.

## Project status

- This repository is intended for **PoC/demo** usage (e.g., blog/demo scenarios).
- The spoke sync Functions host is implemented; the hub event orchestrator host is currently scaffolded only.

## Table of contents

- [Azure Enterprise Platform - Config Management Orchestrator (AEP-CMO)](#azure-enterprise-platform---config-management-orchestrator-aep-cmo)
  - [Project status](#project-status)
  - [Table of contents](#table-of-contents)
  - [What is AEP-CMO?](#what-is-aep-cmo)
  - [What problem does it solve?](#what-problem-does-it-solve)
  - [How it works (high level)](#how-it-works-high-level)
  - [What’s in this repository?](#whats-in-this-repository)
  - [Repository layout](#repository-layout)
  - [Architecture](#architecture)
  - [Getting started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Build](#build)
    - [Run the spoke Functions host locally (Sync Orchestrator)](#run-the-spoke-functions-host-locally-sync-orchestrator)
  - [Configuration](#configuration)
    - [Required environment variables](#required-environment-variables)
  - [Infrastructure (Terraform)](#infrastructure-terraform)
  - [Cost estimate (PoC)](#cost-estimate-poc)
  - [Observability](#observability)
  - [Security](#security)
  - [Contributing](#contributing)
  - [License](#license)
  - [Documentation](#documentation)
  - [Terraform Commands](#terraform-commands)

## What is AEP-CMO?

AEP-CMO is an event-driven solution for **synchronising application configuration and secrets** from a central **Hub** (shared platform subscription) into one or more **Spoke** environments (e.g., Dev/Test, Staging, Production) in an Azure hub-and-spoke enterprise platform.

The goal is to let Platform/Operations manage “golden source” configuration centrally, while workload teams consume configuration locally in each spoke using spoke-owned **Azure App Configuration** and **Azure Key Vault**.

## What problem does it solve?

- **Consistent configuration at scale:** propagate approved configuration changes across many environments.
- **Secret distribution without copying by hand:** synchronise Key Vault-backed settings by copying secrets from hub Key Vault to spoke Key Vault.
- **Decoupled, reliable delivery:** use Azure messaging to fan-out work per spoke and support retries/DLQ.

## How it works (high level)

1. A configuration change occurs in **Hub Azure App Configuration**.
2. The change event is delivered via **Event Grid** to **Hub Service Bus**.
3. The hub-side orchestrator classifies the change (plain value vs Key Vault reference) and publishes a canonical sync message to a **Service Bus Topic**.
4. Each spoke runs an **Azure Functions** app that consumes messages from its own topic subscription and applies the change to:
   - **Spoke App Configuration** (upsert/delete key-values or Key Vault references)
   - **Spoke Key Vault** (create/update/delete secrets sourced from hub Key Vault)
5. The spoke publishes a success/failure result message to a **results/telemetry topic** for monitoring and operations.

## What’s in this repository?

- **Spoke Sync Orchestrator (implemented):**
  - Azure Functions (.NET 8 isolated worker) that performs the sync into spoke App Configuration and Key Vault.
- **Hub Event Orchestrator (scaffolded):**
  - Clean Architecture projects exist (Domain/Application/Infrastructure). A Functions host/entrypoint is not currently present in this repo.
- **Shared libraries:** common abstractions and Azure SDK wrappers for App Configuration, Key Vault, and Service Bus messaging.
- **Terraform:** modules and environment templates to provision the spoke-side infrastructure.

## Repository layout

- `src/Sync.Orchestrator/` – Spoke sync service (Functions host + application/domain/infrastructure)
- `src/Event.Orchestrator/` – Hub event orchestrator (Clean Architecture projects; host not included yet)
- `src/Shared/` – Shared packages (App Configuration, Key Vault, Service Bus, shared domain contracts)
- `terraform/` – Terraform modules and hub/spoke environment templates

## Architecture

- Detailed architecture (data flows, topic names, components): [docs/Architecture.md](docs/Architecture.md)

## Getting started

### Prerequisites

- .NET SDK (pinned via [global.json](global.json))
- Azure Functions Core Tools v4 (only required if you want to run Functions locally)
- Terraform (only required if you want to deploy infrastructure from this repo)
- Azure CLI login and permissions to create resources (for deployments)

### Build

From the repo root:

```powershell
dotnet restore .\aep-cmo.sln
dotnet build .\aep-cmo.sln -c Release
```

### Run the spoke Functions host locally (Sync Orchestrator)

The Functions host lives under:

- `src/Sync.Orchestrator/ConfigManagement.Sync.Orchestrator.Functions`

Typical workflow:

```powershell
cd .\src\Sync.Orchestrator\ConfigManagement.Sync.Orchestrator.Functions

# Create a local.settings.json (not committed) containing the required environment variables.
# Then start the Functions host:
func start
```

Notes:

- `local.settings.json` is intentionally ignored.
- You’ll need a valid Service Bus topic/subscription and App Configuration/Key Vault endpoints to fully exercise the trigger and sync path.

## Configuration

The spoke Functions host reads configuration from **environment variables / app settings** (see `ConfigFactory`).

### Required environment variables

Service metadata:

- `ORGANISATION`
- `REGION`
- `ENVIRONMENT_TIER`
- `ENVIRONMENT_NAME`
- `SERVICE_NAME`

Service Bus (trigger + result publishing):

- `ServiceBus__FullyQualifiedNamespace`
- `SBUS_EVENT_TOPIC`
- `SBUS_EVENT_TOPIC_SUBSCRIPTION`
- `SBUS_EVENT_RESULT_TOPIC`

App Configuration:

- `AppConfiguration__Endpoint`

Key Vault:

- `KeyVault__LocalVaultUri`
- `KeyVault__HubKeyVaultUri`

Storage / idempotency:

- `STORAGE_ACCOUNT_TABLE_URI`
- `STORAGE_ACCOUNT_BLOB_URI`
- `IDEMPOTENCY_TABLE_NAME`

Authentication options (currently required by the config factory):

- `ServiceBus__AuthType`, `ServiceBus__TenantId`, `ServiceBus__ClientId`, `ServiceBus__ClientSecret`, `ServiceBus__ManagedIdentityClientId`
- `AppConfiguration__AuthType`, `AppConfiguration__TenantId`, `AppConfiguration__ClientId`, `AppConfiguration__ClientSecret`, `AppConfiguration__ManagedIdentityClientId`
- `KeyVault__AuthType`, `KeyVault__TenantId`, `KeyVault__ClientId`, `KeyVault__ClientSecret`, `KeyVault__ManagedIdentityClientId`

## Infrastructure (Terraform)

Terraform lives under [terraform/](terraform/).

Environment templates:

- Hub: [terraform/environment_types/hub](terraform/environment_types/hub)
- Spoke: [terraform/environment_types/spoke](terraform/environment_types/spoke)

Each environment folder includes an `example.env.tfvars` you can copy and customise.

Example workflow (spoke):

```powershell
cd .\terraform\environment_types\spoke
terraform init
terraform plan -var-file="example.env.tfvars"
terraform apply -var-file="example.env.tfvars"
```

Example workflow (hub):

```powershell
cd .\terraform\environment_types\hub
terraform init
terraform plan -var-file="example.env.tfvars"
terraform apply -var-file="example.env.tfvars"
```

Notes:

- The hub template provisions hub App Configuration/Key Vault and creates Service Bus topics used for sync and results.
- The spoke template provisions spoke App Configuration/Key Vault and the spoke Function App, and creates a per-spoke Service Bus subscription in the hub namespace.
- Ensure the Function App **app settings names** match the required environment variables listed in [Configuration](#configuration).

## Cost estimate (PoC)

Estimate for **1 month** in **£ (GBP)** using the prices and scenario you provided.

Assumptions used for the estimate:

- 1 Hub + 2 Spokes
- 100 pipeline/terraform runs per day per environment
- 10–50 calls per run across App Configuration + Key Vault (assumed split ~50/50)

| Environment | Service Bus (Std) | Event Grid (Basic) | Functions (Consumption) | App Configuration | Key Vault (Std transactions) | Est. subtotal / month |
|---|---:|---:|---:|---:|---:|---:|
| Hub | ~£8.00 | £0.00 (well under free tier) | £0.00 (well under free tier) | £0.00* | £0.03–£0.17 | ~£8.03–£8.17 |
| Spoke 1 | £0.00 | £0.00 | £0.00 | £0.00* | £0.03–£0.17 | £0.03–£0.17 |
| Spoke 2 | £0.00 | £0.00 | £0.00 | £0.00* | £0.03–£0.17 | £0.03–£0.17 |
| **Total (1 Hub + 2 Spokes)** |  |  |  |  |  | **~£8.09–£8.51 / month** |

\*App Configuration is shown as £0.00 assuming the **Free** tier is sufficient. Under your pipeline assumptions, App Configuration usage is likely **500–2,500 requests/day per environment**, so environments may exceed the **1,000 requests/day** free tier depending on the actual number of App Config calls per run. App Configuration paid-tier cost is not included because only the Free tier limits were provided.

## Observability

- The spoke Functions host is wired for Application Insights.
- Each processed message publishes a success/failure outcome to a Service Bus results/telemetry topic.

## Security

- Designed for **managed identity** and RBAC-based access.
- Do not commit secrets: `local.settings.json` is ignored for local development.

## Contributing

This repo is primarily intended for PoC/demo purposes, but contributions are welcome:

- Open an issue describing the change.
- Keep PRs small and focused.
- For code changes, run `dotnet build`.
- For IaC changes, run `terraform fmt` and validate plans in the relevant environment folder.

## License

MIT licensed. See [LICENSE](LICENSE).

## Documentation

- Terraform/bootstrapping context: [docs/copilot-context.md](docs/copilot-context.md)


## Terraform Commands

terraform init
terraform plan -var-file="env.tfvars" -out="tfplan" -input=false
terraform apply "tfplan"

terraform destroy -var-file="env.tfvars" -auto-approve -input=false