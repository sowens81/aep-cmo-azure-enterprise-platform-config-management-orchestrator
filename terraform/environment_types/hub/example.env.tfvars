# ============================
# Global
# ============================

environment  = "dev"
organisation = "xxuk"

# ============================
# Subscriptions
# ============================

hub_subscription_id         = "00000000-0000-0000-0000-000000000000"
environment_subscription_id = "00000000-0000-0000-0000-000000000000"
tenant_id                   = "00000000-0000-0000-0000-000000000000"

# ============================
# Resource Group / Location
# ============================

resource_group_name = "xxuk-prdhub-uks-rg-configmgmt"
location            = "uksouth"

# ============================
# Function App
# ============================

function_name = "xxuk-prdhub-uks-func-configmgmt"
identity_name = "xxuk-prdhub-uks-mi-configmgmt"

storage_account_name = "stconfigsyncprdhubuks"

# ============================
# App Configuration / Key Vault
# ============================

app_configuration_name = "apcfgconfigsyncprdhubuks"
key_vault_name         = "kvconfigsyncprdhubuks"

app_configuration_sku = "free"

# ============================
# Tags
# ============================

tags = {
  environment = "prdhub"
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
  resource_group_name         = "xxuk-prdhub-uks-rg-configmgmt"
  namespace_name              = "xxuk-prdhub-uks-sbus-configmgmt"
  app_config_sync_topic_name             = "app-config-sync"
  app_config_event_topic_name = "app-config-event"
  key_vault_event_topic_name  = "key-vault-event"
  key_vault_sync_topic_name             = "key-vault-sync"
  result_topic_name           = "app-config-result-telemetry"
}


entraid_spoke_access_group_name = "so-demo-spoke-config-management-consumers-entraid-group"