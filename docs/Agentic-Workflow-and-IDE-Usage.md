# Agentic Development Workflow & IDE Usage Guide

This document defines the **way of working** for developing the ConfigManagement platform using
agentic AI support with **GitHub Copilot**, **VS Code**, and **Visual Studio 2026**.

It describes:
- How specialized agents collaborate
- When each agent should be used
- How humans stay in control
- Practical usage patterns inside the IDEs

This guide is intended for **architects and developers** working in this repository.

---

## Guiding Principles

- Humans own decisions; agents assist execution and review
- One agent = one clear responsibility
- Creation agents build, review agents validate
- Architecture, tests, and IaC are first‑class deliverables
- Explicit prompts beat implicit assumptions

---

## Agent Categories

### Build Agents (Create & Modify)
These agents **author or change artifacts**.

- architecture-designer
- shared-library-engineer
- dotnet-application-engineer
- azure-function-engineer
- infrastructure-iac-engineer
- test-specialist

### Review Agents (Read & Advise Only)
These agents **never introduce new features or logic**.

- architecture-reviewer
- code-quality-reviewer
- test-reviewer
- iac-reviewer

---

## Recommended Agent Workflow

Work proceeds in **phases**.  
Each phase has clear entry and exit criteria.

---

### Phase 0 — Discovery & Intent (Human‑Led)

**Who:** Human architect / developer  
**Goal:** Clarify what needs to be done

Activities:
- Define goal and scope
- Identify impacted services or layers
- Select which agents are required

No agents are invoked in this phase.

---

### Phase 1 — Architecture & Design

**Agents:**
- architecture-designer
- architecture-reviewer

Steps:
1. architecture-designer produces:
   - High‑level architecture
   - Component and data‑flow descriptions
   - Design decisions and trade‑offs
2. architecture-reviewer validates:
   - Clean Architecture compliance
   - Service boundaries
   - Shared package usage
   - Scalability and resiliency

**Exit Criteria:**
- Architecture approved or revised
- No production code written yet

---

### Phase 2 — Shared Foundations (Conditional)

**Agents:**
- shared-library-engineer
- code-quality-reviewer

Triggered only when:
- New shared abstractions are needed
- Existing shared packages require changes

Steps:
- shared-library-engineer updates shared packages and READMEs
- code-quality-reviewer verifies:
  - No service coupling
  - XML documentation completeness
  - API clarity

**Rule:** Shared code must stabilize before service work begins.

---

### Phase 3 — Application & Domain Logic

**Agents:**
- dotnet-application-engineer
- code-quality-reviewer

Steps:
- Implement Domain and Application layers
- Use shared domain models
- Avoid Azure SDK usage
- Ensure XML documentation

Review ensures:
- Correct dependency direction
- Clear intent and naming
- Testability

---

### Phase 4 — Azure Function Host Layer

**Agents:**
- azure-function-engineer
- code-quality-reviewer

Steps:
- Implement isolated Azure Functions (.NET 8, v4)
- Wire triggers, bindings, DI
- Delegate logic to Application layer
- Implement logging, retries, DLQ handling

Review ensures:
- No business logic in functions
- Correct correlation ID usage

---

### Phase 5 — Infrastructure as Code

**Agents:**
- infrastructure-iac-engineer
- iac-reviewer

Steps:
- Write Terraform and Azure Bicep
- Model Service Bus, Event Hub, Functions, Key Vault, App Config, RBAC
- Ensure parity between Terraform and Bicep

Review ensures:
- Security and least privilege
- Environment consistency
- Modularity and reuse

---

### Phase 6 — Testing

**Agents:**
- test-specialist
- test-reviewer

Steps:
- Write unit tests (Domain, Application)
- Write integration tests (Infrastructure)
- Use xUnit and Moq

Review ensures:
- Coverage of key paths and error cases
- Test stability and clarity

---

### Phase 7 — Final System Review

**Agents:**
- architecture-reviewer
- code-quality-reviewer
- test-reviewer
- iac-reviewer

Goal:
- Holistic validation before merge
- Ensure production readiness

---

## VS Code & GitHub Copilot Usage Patterns

---

### Explicit Agent Invocation Pattern

Always invoke agents explicitly in Copilot Chat.

Example:
```
Use the architecture-designer agent.

Context:
- Service: ConfigManagement.Sync.Orchestrator
- Goal: design delete‑event processing flow
- Constraints: reuse shared domain models

Output:
- Architecture notes
- Sequence description
- Design decisions
```

---

### File‑Scoped Editing Pattern

1. Open a file
2. Select relevant code
3. Ask Copilot:

```
Using the dotnet-application-engineer agent,
refactor this code for clarity and testability.
Do not change public behavior.
```

This prevents unintended global changes.

---

### Review‑Only Pattern

For pull requests and large changes:

```
Use the code-quality-reviewer agent.

Review only.
Do not introduce new features.
Identify architectural or Clean Architecture violations.
```

---

### Test‑First / Test‑After Pattern

After implementing logic:

```
Use the test-specialist agent.

Write unit tests for the selected Application code.
Use xUnit and Moq.
Do not modify production code.
```

---

### Infrastructure Safety Pattern

Before merging IaC:

```
Use the iac-reviewer agent.

Review Terraform and Bicep files for:
- Security risks
- RBAC overreach
- Environment coupling
```

---

## Visual Studio 2026 Usage

Use **Visual Studio** for:
- Debugging Azure Functions
- Running and inspecting tests
- Dependency and architecture visualization

Recommended split:
- VS Code + Copilot → design, writing, refactoring
- Visual Studio → execution, debugging, validation

---

## Pull Request Expectations

Recommended PR checklist:

```markdown
## Agents Used
- architecture-designer
- dotnet-application-engineer
- test-specialist
- code-quality-reviewer

## Architecture Impact
- [ ] None
- [ ] Minor
- [ ] Significant (docs updated)

## Tests
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated

## IaC Impact
- [ ] None
- [ ] Terraform
- [ ] Bicep
```

---

## Summary

This workflow ensures:
- Clear accountability between humans and agents
- Strong architectural governance
- High code and test quality
- Safe, reviewable infrastructure changes

Agents are collaborators, not decision makers.
Humans remain responsible for outcomes.
