# Copilot Context – Config & Secret Sync Bootstrapper

You are assisting with a Terraform-based Azure bootstrapper for a **spoke subscription**.

## Architecture Summary

This system syncs configuration and secrets from a **hub** subscription to a **spoke** subscription using an event-driven model.

### Message Types
Messages arrive on an Azure Service Bus Topic and have one of two forms:

1. key:value  
   - Write or update key/value in **spoke Azure App Configuration**

2. key:kvid  
   - If key exists in spoke App Configuration:
     - Read secret from **hub Key Vault** using the provided Key Vault Secret URI
     - Write secret to **spoke Key Vault** with the same name
   - If key does NOT exist:
     - Read secret from hub Key Vault
     - Write secret to spoke Key Vault
     - Add key to spoke App Configuration as a **Key Vault reference** pointing to the spoke Key Vault secret

All operations are idempotent.

---

## Microservices (Azure Functions)

Each microservice is implemented as an **Azure Function**:

1. Service Bus Listener  
   - Triggered by Service Bus Topic  
   - Validates message schema  
   - Ensures correlationId exists  
   - Routes internally  

2. Config Writer  
   - Handles key:value messages  
   - Writes to spoke App Configuration  

3. Secret Sync Orchestrator  
   - Handles key:kvid messages  
   - Coordinates checks and sync logic  

4. Key Vault Access  
   - Reads secrets from hub Key Vault  
   - Writes secrets to spoke Key Vault  

5. App Configuration Access  
   - Reads and writes keys in spoke App Configuration  

---

## Observability & Reliability

- Every message has a `correlationId`
- Correlation IDs are logged and propagated everywhere
- A **Sync Result Service Bus Topic** emits SUCCESS/FAILED events
- Sync metadata is stored (last synced secret version, timestamps, status)

---

## Security Model

- **Managed Identity only** (no secrets, no connection strings)
- One **User-Assigned Managed Identity** shared by all Functions
- Hub access is granted via Entra ID group:
  - hub-config-sync-reader-group
  - Group has:
    - Service Bus Topic Data Receiver (hub)
    - Key Vault Secrets Reader (hub)

---

## Spoke Resources (Terraform-managed)

Terraform creates ONLY spoke-side resources:

- Azure Functions (Linux, Consumption)
- User-Assigned Managed Identity
- Spoke Azure App Configuration
- Spoke Azure Key Vault (RBAC enabled)
- Storage Account for Functions
- App Service Plan (Y1)

---

## RBAC Requirements (Spoke)

Managed Identity has:
- App Configuration Data Owner (spoke App Config)
- Key Vault Secrets Officer (spoke Key Vault)

Terraform MUST NOT create:
- Hub resources
- Service Bus topics
- Secrets or config keys

---

## Constraints & Style

- Terraform only
- AzureRM provider (>= 3.80)
- Least-privilege RBAC
- Clear separation of concerns
- Idempotent, production-grade code
- Prefer modules and reusable patterns