data "azurerm_client_config" "current" {}

# Lookup built-in role definitions to avoid relying on localized role names
data "azurerm_role_definition" "appconfig_data_owner" {
  name  = "App Configuration Data Owner"
  scope = "/subscriptions/${data.azurerm_client_config.current.subscription_id}"
}

data "azurerm_role_definition" "kv_secrets_officer" {
  name  = "Key Vault Secrets Officer"
  scope = "/subscriptions/${data.azurerm_client_config.current.subscription_id}"
}

data "azurerm_role_definition" "kv_secrets_user" {
  name  = "Key Vault Secrets User"
  scope = "/subscriptions/${data.azurerm_client_config.current.subscription_id}"
}