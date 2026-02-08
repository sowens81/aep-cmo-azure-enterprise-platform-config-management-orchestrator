# Agent Knowledge Baseline – Pre-Agent State

This document is a **condensed knowledge file** derived from the full
`pre-agent-baseline.md` snapshot. It is intended to be the **primary onboarding
and context file for agents** so they understand what already exists and what
was in progress **before agent-driven development began**.

This file is optimized for:
- Agent consumption
- Human architectural orientation
- Avoiding rework or duplicate foundations

> The full raw snapshot remains authoritative and should be referenced for
> exact file contents and history.

---

## Snapshot Metadata

- **Snapshot Date:** 2026-02-08
- **Purpose:** Preserve repository state before agent adoption
- **Source File:** `docs/pre-agent-baseline.md`

---

## Solution Overview

### Solution Name
**ConfigManagement.sln**

### Technology Stack
- .NET 8
- Azure Functions v4 (Isolated Worker)
- Azure Service Bus (Topics & Subscriptions + DLQ)
- Azure Event Hub
- Azure App Configuration
- Azure Key Vault
- Terraform (Hub & Spoke environments)

---

## High-Level Architecture (Current State)

The solution already follows **Clean Architecture** with clear separation of:

- Domain
- Application
- Infrastructure
- Host (Azure Functions)

Two orchestrator services exist:
1. **Hub / Event Orchestrator**
2. **Spoke / Sync Orchestrator**

Shared abstractions are centralized in `ConfigManagement.Shared`.

---

## Existing Services

### ConfigManagement.Event.Orchestrator (Hub)

**Location:**
`src/Event.Orchestrator`

**Projects Present:**
- Application
- Domain
- Infrastructure

**Purpose (Established):**
- Receive App Configuration change events
- Prepare and classify events for downstream processing
- Act as the upstream event source

**Status:**
- Project structure exists
- Build artifacts present
- Core orchestration logic partially implemented

---

### ConfigManagement.Sync.Orchestrator (Spoke)

**Location:**
`src/Sync.Orchestrator`

**Projects Present:**
- Application
- Domain
- Infrastructure
- Functions (Host)

**Key Functional Areas Already Present:**
- Handlers
- Orchestration logic
- Service abstractions
- Idempotency handling
- App Configuration integration
- Key Vault integration
- Service Bus integration

**Status:**
- Most structural foundations exist
- Domain models, enums, and result objects are defined
- Functions host wired to application layer

---

## Shared Libraries (Strong Foundation Already Exists)

**Location:**
`src/Shared`

### Existing Packages

#### ConfigManagement.Shared.Domain
- Shared enums
- Models
- Result objects
- Error definitions
- Intended as the canonical contract layer

#### ConfigManagement.Shared.ServiceBus
- Authentication helpers
- Messaging models
- Service Bus abstractions

#### ConfigManagement.Shared.AppConfiguration
- App Configuration access patterns
- Options and authentication abstractions

#### ConfigManagement.Shared.KeyVault
- Key Vault authentication
- Options and secret access abstractions

**Important Note for Agents:**
These packages **must be reused**, not reimplemented.

---

## Infrastructure as Code (Terraform)

**Location:**
`/terraform`

### Structure
- Environment types:
  - `hub`
  - `spoke`
- Modular design:
  - app_configuration
  - key_vault
  - linux_function_app
  - service_bus
  - service_bus_subscription

**Status:**
- Terraform providers initialized
- Modules exist and are structured for reuse
- Azure resources already defined at a foundational level

---

## Agent System Already Present

**Location:**
`.github/agents`

### Agents Already Defined
- architecture-designer
- architecture-reviewer
- azure-function-engineer
- dotnet-application-engineer
- shared-library-engineer
- infrastructure-iac-engineer
- test-specialist
- test-reviewer
- code-quality-reviewer
- iac-reviewer

**Important:**
These agents were introduced **after** this snapshot.
They should treat this repository state as **baseline truth**.

---

## Known Gaps at Snapshot Time

These areas were **not yet fully implemented** at snapshot time:

- Comprehensive unit test coverage
- Integration tests
- Azure Bicep IaC equivalents
- Finalized architecture documentation
- End-to-end message flow validation
- Operational dashboards / telemetry

Agents should **continue**, not restart, these efforts.

---

## Rules for Agents Going Forward

Agents must:
- Assume all listed structures are intentional
- Extend existing abstractions rather than replace them
- Preserve Clean Architecture boundaries
- Avoid duplicating shared functionality
- Add tests alongside new behavior
- Update documentation when architecture changes

Agents must NOT:
- Flatten or bypass existing layers
- Recreate shared libraries
- Introduce service-specific logic into shared packages
- Treat this as a greenfield project

---

## How Agents Should Use This File

- Use this document as **initial orientation**
- Use `pre-agent-baseline.md` for detailed inspection
- Reference this file when deciding:
  - Where to add new code
  - Which abstractions already exist
  - What work is continuation vs new

---

## Human Guidance

If ambiguity exists:
- Prefer extending existing structures
- Ask for clarification rather than redesign
- Treat shared domain models as contracts

This project is **mid-flight**, not at inception.
