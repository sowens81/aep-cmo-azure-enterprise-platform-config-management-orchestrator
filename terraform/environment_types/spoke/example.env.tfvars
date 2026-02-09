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

resource_group_name = "xxuk-dev-uks-rg-configmgmt"
location            = "uksouth"

# ============================
# Function App
# ============================

function_name = "xxuk-dev-uks-func-configmgmt"
identity_name = "xxuk-dev-uks-mi-configmgmt"

storage_account_name = "stconfigsyncdevuks"

# ============================
# App Configuration / Key Vault
# ============================

app_configuration_name = "apcfgconfigsyncdevuks"
key_vault_name         = "kvconfigsyncdevuks"

app_configuration_sku = "Standard"

# ============================
# Tags
# ============================

tags = {
  environment = "dev"
  workload    = "config-sync"
  owner       = "Steve Owens"
  type        = "proof of concept"
  region      = "uksouth"

}

# ============================
# Entra ID / Hub Access
# ============================

hub_entra_id_group = "22222222-2222-2222-2222-222222222222"

hub_key_vault_uri = "https://kv-hub-shared.vault.azure.net/"

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
  result_topic_name          = "app-config-sync-telemetry"
}
