# ============================
# Global
# ============================

environment_tier = "production"
environment  = "prod"
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

resource_group_name = "xxuk-prod-uks-rg-configmgmt"
location            = "uksouth"

# ============================
# Function App
# ============================

function_name = "xxuk-prod-uks-func-configmgmt"
identity_name = "xxuk-prod-uks-mi-configmgmt"

storage_account_name = "stconfigsyncproduks"

# ============================
# App Configuration / Key Vault
# ============================

app_configuration_name = "apcfgconfigproduks"
key_vault_name         = "kvconfigproduks"

app_configuration_sku = "standard"

# ============================
# Tags
# ============================

tags = {
  environment = "prod"
  workload    = "config-sync"
  owner       = "First Name Surname"
  type        = "proof of concept"
  region      = "uksouth"

}

# ============================
# Entra ID / Hub Access
# ============================

hub_entra_id_group = "demo-spoke-config-management-consumers-entraid-group"

hub_key_vault_uri = "https://kvconfigsyncprdhubuks.vault.azure.net/"

# ============================
# Idempotency
# ============================

idempotency_table = "configSyncIdempotency"

# ============================
# Service Bus (Hub)
# ============================

servicebus_config = {
  resource_group_name        = "xxuk-prdhub-uks-rg-configmgmt"
  namespace_name             = "xxuk-prdhub-uks-sbus-configmgmt"
  app_config_sync_topic_name = "app-config-sync"
  key_vault_sync_topic_name  = "key-vault-sync"
  result_topic_name          = "app-config-result-telemetry"
}
