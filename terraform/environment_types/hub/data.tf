data "azurerm_client_config" "current" {}

data "azurerm_role_definition" "appconfig_data_owner" {
  name  = "App Configuration Data Owner"
  scope = "/subscriptions/${data.azurerm_client_config.current.subscription_id}"
}

data "azurerm_role_definition" "appconfig_data_reader" {
  name  = "App Configuration Data Reader"
  scope = "/subscriptions/${data.azurerm_client_config.current.subscription_id}"
}

data "azurerm_role_definition" "kv_secrets_officer" {
  name  = "Key Vault Secrets Officer"
  scope = "/subscriptions/${data.azurerm_client_config.current.subscription_id}"
}

data "azurerm_role_definition" "kv_secrets_reader" {
  name  = "Key Vault Secrets User"
  scope = "/subscriptions/${data.azurerm_client_config.current.subscription_id}"
}

data "azurerm_role_definition" "sbus_data_owner" {
  name  = "Azure Service Bus Data Owner"
  scope = "/subscriptions/${data.azurerm_client_config.current.subscription_id}"
}

data "azurerm_role_definition" "sbus_data_sender" {
  name  = "Azure Service Bus Data Sender"
  scope = "/subscriptions/${data.azurerm_client_config.current.subscription_id}"
}

data "azurerm_role_definition" "sbus_data_receiver" {
  name  = "Azure Service Bus Data Receiver"
  scope = "/subscriptions/${data.azurerm_client_config.current.subscription_id}"
}