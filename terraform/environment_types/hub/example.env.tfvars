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
  owner       = "<OWNER_NAME_OR_TEAM>"
}


# ============================
# Service Bus (Hub)
# ============================

servicebus_config = {
  namespace_name              = "<YOUR_SERVICE_BUS_NAMESPACE>"
  app_config_sync_topic_name  = "app-config-sync"
  key_vault_sync_topic_name   = "key-vault-sync"
  app_config_event_topic_name = "app-config-event"
  key_vault_event_topic_name  = "key-vault-event"
}

entraid_spoke_access_group_name = "<HUB_ENTRAID_SPOKE_ACCESS_GROUP>"

local_development = true

