# Gap Analysis – Pre-Agent State vs Agent Operating Model

This document captures a **gap analysis** between:

- The **current repository state** as captured in `Agent-Knowledge-Baseline.md`
- The **intended agent-driven operating model** (architecture, delivery, quality)

The purpose is to:
- Make gaps explicit
- Prevent agents from redoing existing work
- Convert gaps into **clear, actionable tasks/prompts** for agents and humans

This document should be treated as a **living backlog** for agent-driven work.

---

## Scope & Reference

**Baseline Inputs:**
- `docs/pre-agent-baseline.md`
- `docs/Agent-Knowledge-Baseline.md`

**Target State Inputs:**
- Agent definitions in `.github/agents`
- Agentic workflow and IDE usage guide
- Clean Architecture + testing + IaC expectations

---

## Summary of Current State

### What Is Already in Place
- Clean Architecture–aligned solution structure
- Hub and Spoke orchestrator services created
- Shared libraries for:
  - Domain
  - Service Bus
  - App Configuration
  - Key Vault
- Terraform infrastructure modules (hub & spoke)
- Agent system defined and committed
- Azure Functions isolated worker (.NET 8) projects wired

### What Is Partially Implemented
- Business orchestration logic
- Error/result models
- Messaging abstractions
- Infrastructure wiring


### What Is Largely Missing
- Tests
- Architecture documentation
- Azure Bicep IaC parity
- Formal review enforcement
- Operational documentation
- Idempotency mechanisms
- Dead Letter queue (dlq) handling

---

## Gap Categories

Each gap is listed with:
- **Gap Description**
- **Impact**
- **Recommended Agent**
- **Suggested Prompt / Task**

---

## GAP 1 – Architecture Documentation Missing

**Description:**
Architecture exists implicitly in code but lacks formal documentation (C4, flows, decisions).

**Impact:**
- Hard for new agents or humans to reason about intent
- High risk of accidental architectural drift

**Recommended Agent:**
- architecture-designer
- architecture-reviewer

**Suggested Prompt:**
```text
Use the architecture-designer agent.

Create architecture documentation for the current system:
- C4 Level 1–3 descriptions
- Message flow between Hub and Spoke
- Key design decisions already reflected in code

Use Agent-Knowledge-Baseline.md as authoritative context.
Do not redesign existing architecture.
```

---

## GAP 2 – Unit Test Coverage (Domain & Application)

**Description:**
Minimal to no unit tests exist for Domain and Application layers.

**Impact:**
- Low confidence in refactoring
- Harder for agents to safely extend logic

**Recommended Agent:**
- test-specialist
- test-reviewer

**Suggested Prompt:**
```text
Use the test-specialist agent.

Add unit tests for existing Domain and Application layers:
- Focus on business rules and decision logic
- Use xUnit and Moq
- Do not modify production code

Use existing models and handlers as-is.
```

---

## GAP 3 – Integration Tests for Infrastructure

**Description:**
Infrastructure interactions are not validated with integration tests.

**Impact:**
- Runtime-only failures
- Risky Azure SDK usage

**Recommended Agent:**
- test-specialist
- test-reviewer

**Suggested Prompt:**
```text
Use the test-specialist agent.

Create integration tests for Infrastructure components:
- Service Bus
- App Configuration
- Key Vault

Use test doubles or Azure emulators where appropriate.
Do not add new features.
```

---

## GAP 4 – Azure Bicep IaC Parity

**Description:**
Terraform exists; Azure Bicep equivalents are missing.

**Impact:**
- Inconsistent deployment options
- Violates stated delivery expectations

**Recommended Agent:**
- infrastructure-iac-engineer
- iac-reviewer

**Suggested Prompt:**
```text
Use the infrastructure-iac-engineer agent.

Create Azure Bicep equivalents for existing Terraform modules:
- Preserve structure and naming
- Ensure functional parity
- Do not redesign infrastructure

Use existing Terraform modules as the source of truth.
```

---

## GAP 5 – Results & Telemetry Standardization

**Description:**
Result publishing exists but lacks a standardized schema and documentation.

**Impact:**
- Hard to build dashboards or alerting
- Inconsistent operational visibility

**Recommended Agent:**
- shared-library-engineer
- architecture-reviewer

**Suggested Prompt:**
```text
Use the shared-library-engineer agent.

Review existing result and error models.
Standardize result contracts for:
- Success
- Retry
- DLQ
- Discard

Update shared domain models and documentation only.
```

---

## GAP 6 – Agent Review Enforcement

**Description:**
Review agents exist but are not enforced via process or documentation.

**Impact:**
- Agents may bypass quality gates
- Inconsistent outcomes

**Recommended Agent:**
- architecture-reviewer
- code-quality-reviewer

**Suggested Prompt:**
```text
Use the architecture-reviewer agent.

Define mandatory review checkpoints for:
- Architecture changes
- Production code
- Tests
- IaC

Update the way-of-working documentation.
```

---

## GAP 7 – Operational & Runbook Documentation

**Description:**
No operational guidance or failure-handling runbooks exist.

**Impact:**
- Difficult incident response
- Poor on-call readiness

**Recommended Agent:**
- architecture-designer

**Suggested Prompt:**
```text
Use the architecture-designer agent.

Create operational documentation covering:
- Message retry behavior
- DLQ handling
- Failure modes
- Manual recovery steps

Base this on existing implementation.
```

---

## GAP 8 – Agent Context Reinforcement

**Description:**
Agents may not always be reminded that this is a mid-flight project.

**Impact:**
- Risk of greenfield assumptions
- Duplicate abstractions

**Recommended Agent:**
- code-quality-reviewer

**Suggested Prompt:**
```text
Use the code-quality-reviewer agent.

Review recent changes and validate:
- No duplicated shared abstractions
- No Clean Architecture violations
- No reimplementation of existing logic

Reference Agent-Knowledge-Baseline.md explicitly.
```

---

## Prioritization Recommendation

**High Priority:**
1. Architecture documentation
2. Unit tests
3. Integration tests

**Medium Priority:**
4. Bicep IaC
5. Result standardization

**Ongoing:**
6. Review enforcement
7. Operational docs
8. Context reinforcement

---

## How to Use This Document

- Treat each GAP as a **task backlog item**
- Convert suggested prompts directly into Copilot Chat requests
- Close gaps incrementally
- Update this document as gaps are closed

---

## Final Note

This is not a failure list — it is a **controlled continuation plan**.

The foundation is strong.
Agents now have clear direction on **where to help next**.
