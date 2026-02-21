# ============================
# Global
# ============================

environment_tier = "<devtest|production>"
environment      = "<YOUR_ENVIRONMENT_NAME>"
organisation     = "<YOUR_ORGANISATION>"

# ============================
# Subscriptions
# ============================

hub_subscription_id         = "<HUB_SUBSCRIPTION_ID>"
environment_subscription_id = "<ENVIRONMENT_SUBSCRIPTION_ID>"
tenant_id                   = "<TENANT_ID>"

# ============================
# Resource Group / Location
# ============================

resource_group_name = "<YOUR_RESOURCE_GROUP_NAME>"
location            = "<YOUR_LOCATION>"

# ============================
# Function App
# ============================

function_name = "<YOUR_FUNCTION_APP_NAME>"
identity_name = "<YOUR_MANAGED_IDENTITY_NAME>"

storage_account_name = "<YOUR_STORAGE_ACCOUNT_NAME>"

# ============================
# App Configuration / Key Vault
# ============================

app_configuration_name = "<YOUR_APP_CONFIGURATION_NAME>"
key_vault_name         = "<YOUR_KEY_VAULT_NAME>"

app_configuration_sku = "free"

# ============================
# Tags
# ============================

tags = {
  environment = "<YOUR_ENVIRONMENT_TAG>"
  workload    = "config-sync"
  owner       = "<OWNER_NAME>"
  type        = "<TYPE>"
  region      = "<YOUR_REGION>"

}

# ============================
# Entra ID / Hub Access
# ============================

hub_entra_id_group = "<HUB_ENTRA_ID_GROUP>"

hub_key_vault_uri = "https://<YOUR_HUB_KEY_VAULT_NAME>.vault.azure.net/"

hub_app_configuration_uri = "https://<YOUR_HUB_APP_CONFIGURATION>.azconfig.io"

# ============================
# Service Bus (Hub)
# ============================

servicebus_config = {
  resource_group_name        = "<HUB_RESOURCE_GROUP_NAME>"
  namespace_name             = "<YOUR_SERVICE_BUS_NAMESPACE>"
  app_config_sync_topic_name = "app-config-sync"
  key_vault_sync_topic_name  = "key-vault-sync"
}

local_development = true