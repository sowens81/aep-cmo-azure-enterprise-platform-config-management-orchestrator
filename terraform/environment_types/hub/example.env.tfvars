# ============================
# Global
# ============================

environment  = "dev"
organisation = "xxuk"

# ============================
# Subscriptions
# ============================

hub_subscription_id         = "00000000-0000-0000-0000-000000000000"
environment_subscription_id = "11111111-1111-1111-1111-111111111111"

# ============================
# Resource Group / Location
# ============================

resource_group_name = "xxuk-prod-uks-rg-config_sync_hub_orchestrator"
location            = "uksouth"

# ============================
# Function App
# ============================

function_name = "xxuk-prod-uks-func-config_sync_hub_orchestrator"
identity_name = "xxuk-prod-uks-mi-config_sync_hub_orchestrator"

storage_account_name = "stconfigsyncproduks"

# ============================
# App Configuration / Key Vault
# ============================

app_configuration_name = "apcfgconfigsyncproduks"
key_vault_name         = "kvconfigsyncproduks"

app_configuration_sku = "Standard"

# ============================
# Tags
# ============================

tags = {
  environment = "prod"
  workload    = "config-sync"
  owner       = "platform-team"
}


# ============================
# Idempotency
# ============================

idempotency_table = "configSyncIdempotency"

# ============================
# Service Bus (Hub)
# ============================

servicebus_config = {
  resource_group_name = "xxuk-prod-uks-rg-config_sync_hub_orchestrator"
  namespace_name      = "xxuk-prod-uks-sbus-config_sync_hub_orchestrator"
  sync_topic_name     = "app-config-sync"
  event_topic_name     = "app-config-event"
  result_topic_name   = "app-config-event-telemetry"
}
