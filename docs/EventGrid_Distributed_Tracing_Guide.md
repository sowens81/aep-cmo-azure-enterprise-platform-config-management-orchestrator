# Event Grid → Topics → Functions → Database

## Complete Guide to Correlation, Tracing, and Message Design

------------------------------------------------------------------------

# 1. Architecture Overview

``` mermaid
flowchart LR
    EG[Event Grid Event]
    F1[Function A]
    T1[Topic A]
    F2[Function B]
    T2[Topic B]
    F3[Function C]
    DB[(Database)]

    EG --> F1
    F1 --> T1
    T1 --> F2
    F2 --> T2
    T2 --> F3
    F3 --> DB
```

------------------------------------------------------------------------

# 2. The Identifiers Explained

  ------------------------------------------------------------------------
  Identifier         Purpose         Lifetime        Created When
  ------------------ --------------- --------------- ---------------------
  EventGridEventId   Identifies the  Entire flow     From Event Grid
                     original                        
                     external event                  

  CorrelationId      Identifies the  Entire flow     First consumer
                     business                        
                     workflow                        

  TraceId            Identifies      Entire flow     First consumer
                     distributed                     (framework)
                     trace                           

  SpanId             Identifies one  Per operation   Every service step
                     processing step                 
  ------------------------------------------------------------------------

------------------------------------------------------------------------

# 3. Identifier Format Rules (IMPORTANT)

## TraceId and SpanId MUST Follow W3C Trace Context

TraceId and SpanId are NOT GUIDs.

They must follow the W3C Trace Context specification.

### TraceId

-   16 bytes
-   32 hexadecimal characters
-   No hyphens
-   Cannot be all zeros

Example:

    4bf92f3577b34da6a3ce929d0e0e4736

### SpanId

-   8 bytes
-   16 hexadecimal characters
-   No hyphens
-   Cannot be all zeros

Example:

    00f067aa0ba902b7

### Why NOT GUID?

A GUID looks like:

    d290f1ee-6c54-4b01-90e6-d701748f0851

Problems: - Contains hyphens - Wrong encoding format - Not compliant
with W3C traceparent - Breaks distributed tracing tools (OpenTelemetry,
App Insights, etc.)

### Correct Usage

DO NOT manually create TraceId or SpanId.

Let the framework generate them:

-   .NET: System.Diagnostics.Activity
-   OpenTelemetry SDK
-   Azure Functions runtime

------------------------------------------------------------------------

# 4. What Gets Created Where

## Stage 1 -- Event Grid Emits Event

Event Grid message contains:

    id = 84e17ea4-66db-4b54-8050-df8f7763f87b

At this point: - EventGridEventId = present - CorrelationId = NOT
present - TraceId = NOT present - SpanId = NOT present

------------------------------------------------------------------------

## Stage 2 -- Function A (First Consumer)

This is the TRACE ROOT.

### Create:

-   CorrelationId = new GUID (business identifier)
-   TraceId = created by framework
-   SpanId = created by framework (root span)

### Extract:

-   EventGridEventId = event.id

### Log Example

    Processing EventGrid event
    EventGridEventId=84e17ea4-66db...
    CorrelationId=9c11b5c2...
    TraceId=4bf92f3577b34da6a3ce929d0e0e4736
    SpanId=a1b2c3d4e5f6

------------------------------------------------------------------------

## Stage 3 -- Function A Publishes to Topic A

### Message Headers Should Include:

    EventGridEventId
    CorrelationId
    traceparent (contains TraceId + current SpanId)

Example:

    EventGridEventId = 84e17ea4-66db...
    CorrelationId = 9c11b5c2...
    traceparent = 00-4bf92f3577b34da6a3ce929d0e0e4736-a1b2c3d4e5f6-01

------------------------------------------------------------------------

## Stage 4 -- Function B Consumes from Topic A

### Extract:

-   EventGridEventId
-   CorrelationId
-   TraceId (from traceparent)
-   ParentSpanId

### Create:

-   New SpanId (framework)

``` mermaid
sequenceDiagram
    participant F1 as Function A
    participant F2 as Function B

    F1->>F2: Message (TraceId=T, SpanId=S1)
    F2->>F2: Create new SpanId=S2 (parent=S1)
```

------------------------------------------------------------------------

## Stage 5 -- Function B Publishes to Topic B

Forward unchanged:

-   EventGridEventId
-   CorrelationId
-   TraceId

New consumer will create new SpanId.

------------------------------------------------------------------------

## Stage 6 -- Function C Consumes and Writes to Database

### Create:

-   SpanId for function execution
-   SpanId for database call

``` mermaid
flowchart TD
    F3Span[Function C Span S3]
    DBSpan[Database Span S4]

    F3Span --> DBSpan
```

------------------------------------------------------------------------

# 5. What to Include in Logs (Best Practice)

Every log line should include:

-   EventGridEventId
-   CorrelationId
-   TraceId
-   SpanId
-   ParentSpanId (if applicable)
-   MessageId (if using Service Bus)
-   Timestamp
-   Service Name

------------------------------------------------------------------------

# 6. What to Include in Topic Messages

## Required Headers

-   EventGridEventId
-   CorrelationId
-   traceparent

## Optional

-   MessageType
-   SchemaVersion
-   CausationId (advanced patterns)

------------------------------------------------------------------------

# 7. Retry Behavior

If a message is retried:

-   EventGridEventId remains same.
-   CorrelationId remains same.
-   TraceId remains same.
-   New SpanId is created for retry attempt.

------------------------------------------------------------------------

# 8. Golden Rules

1.  CorrelationId is a business identifier (GUID is fine).
2.  TraceId and SpanId must follow W3C format.
3.  Never manually generate TraceId/SpanId unless implementing your own
    tracing system.
4.  SpanId is always new per service operation.
5.  EventGridEventId is immutable and always propagated.

------------------------------------------------------------------------

# End of Guide
