# Distributed Tracing ID Best Practices (Agent Knowledge)

This document defines best practices for **trace IDs, span IDs, and telemetry context propagation**
in a microservices architecture. It is intended to be used as **agent knowledge** and internal
documentation for engineering teams.

---

## Core Principles

### 1. One Trace ID per End-to-End Request
- A **single Trace ID** represents one request or workflow from entry to completion.
- The Trace ID is **generated once** at the system boundary:
  - API Gateway
  - Edge service
  - Message consumer (for async systems)
- The same Trace ID is propagated across **all downstream services**.

**Why**
- Enables full end-to-end visibility
- Prevents trace fragmentation
- Makes latency and error analysis meaningful

---

### 2. One Span per Operation
- Each service creates its own **Span ID**
- Spans represent:
  - Incoming requests
  - Internal operations
  - Outgoing calls
- Parent–child relationships define execution flow

**Mental model**
- Trace ID = entire request lifecycle
- Span IDs = individual steps within that lifecycle

---

### 3. Use Automatic Context Propagation
Avoid manually passing trace identifiers in application logic.

**Best practice**
- Use telemetry SDKs and middleware (e.g., OpenTelemetry)
- Let frameworks inject and extract context automatically

**Propagation examples**
- HTTP / gRPC: headers
- Messaging: message headers or metadata
- Background jobs: execution context

---

## Standards and Interoperability

### 4. Follow W3C Trace Context
Use the W3C Trace Context specification for propagation.

Common headers:
- `traceparent`
- `tracestate`

**Benefits**
- Vendor-neutral
- Cross-language compatible
- Supported by most observability platforms

---

### 5. Do Not Reuse Trace IDs
- Never reuse a Trace ID across multiple requests
- Even if requests are related, create **new traces**

If correlation is required:
- Use **trace links**
- Or use business identifiers as attributes

---

## Business Context vs Telemetry Context

### 6. Keep Business IDs Separate
Trace IDs are **technical identifiers**, not business identifiers.

Attach business data as attributes or log fields:
- `order_id`
- `user_id`
- `tenant_id`
- `transaction_id`

**Why**
- Improves trace searchability
- Avoids semantic misuse of Trace IDs
- Keeps traces portable across tools

---

## Asynchronous and Event-Driven Systems

### 7. Trace Continuation vs Trace Linking

**Continue the trace when**
- Async work is part of the same logical request
- Latency continuity matters

**Create a new trace and link when**
- Work is delayed or scheduled
- Events are loosely coupled
- Long-running workflows would create oversized traces

---

## Sampling Strategy

### 8. Sample at the Entry Point
- Make the sampling decision **once**
- Propagate that decision downstream

Avoid:
- Independent sampling per service
- Partial traces caused by dropped spans

---

## Anti-Patterns to Avoid

- Generating new Trace IDs in every service
- Using Trace IDs as database primary keys
- Logging Trace IDs without propagating context
- Mixing correlation IDs and Trace IDs without clear semantics

---

## Summary (Rules of Thumb)

- One Trace ID per request
- One Span ID per operation
- Automatic propagation only
- W3C Trace Context everywhere
- Business identifiers as attributes, not Trace IDs

---

## Recommended Tooling
- OpenTelemetry SDKs
- Vendor-agnostic exporters
- Centralized sampling configuration

This document should be treated as **canonical guidance** for telemetry and tracing decisions.
