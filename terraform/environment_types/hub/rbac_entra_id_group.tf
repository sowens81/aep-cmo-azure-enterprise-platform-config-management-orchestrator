# Entraid Group RBAC
resource "azuread_group" "this" {
  display_name     = var.entraid_spoke_access_group_name
  owners           = [data.azurerm_client_config.current.object_id]
  security_enabled = true
}

## Key Vault Secrets User
resource "random_uuid" "entraid_group_role_kv" {
  keepers = {
    principal = azuread_group.this.object_id
    scope     = module.key_vault.id
  }
}

resource "azurerm_role_assignment" "entraid_group_kv_secrets_user" {
  scope              = module.key_vault.id
  role_definition_id = data.azurerm_role_definition.kv_secrets_user.id
  principal_id       = azuread_group.this.object_id
  name               = random_uuid.entraid_group_role_kv.result
}

## App Configuration Data Reader
resource "random_uuid" "entraid_group_role_appconfig" {
  keepers = {
    principal = azuread_group.this.object_id
    scope     = module.app_configuration.id
  }
}

resource "azurerm_role_assignment" "entraid_group_appconfig_data_reader" {
  scope              = module.app_configuration.id
  role_definition_id = data.azurerm_role_definition.appconfig_data_reader.id
  principal_id       = azuread_group.this.object_id
  name               = random_uuid.entraid_group_role_appconfig.result
}

## Service Bus Data Receiver - App Config Sync Topic
resource "random_uuid" "entraid_group_role_servicebus_data_receiver_appconfig" {
  keepers = {
    principal = azuread_group.this.object_id
    scope     = module.servicebus.topics[var.servicebus_config.app_config_sync_topic_name]
  }
}

resource "azurerm_role_assignment" "entraid_group_servicebus_data_receiver_appconfig" {
  scope              = module.servicebus.topics[var.servicebus_config.app_config_sync_topic_name]
  role_definition_id = data.azurerm_role_definition.sbus_data_receiver.id
  principal_id       = azuread_group.this.object_id
  name               = random_uuid.entraid_group_role_servicebus_data_receiver_appconfig.result
}

## Service Bus Data Receiver - Key Vault Sync Topic
resource "random_uuid" "entraid_group_role_servicebus_data_receiver_keyvault" {
  keepers = {
    principal = azuread_group.this.object_id
    scope     = module.servicebus.topics[var.servicebus_config.key_vault_event_topic_name]
  }
}

resource "azurerm_role_assignment" "entraid_group_servicebus_data_receiver_keyvault" {
  scope              = module.servicebus.topics[var.servicebus_config.key_vault_event_topic_name]
  role_definition_id = data.azurerm_role_definition.sbus_data_receiver.id
  principal_id       = azuread_group.this.object_id
  name               = random_uuid.entraid_group_role_servicebus_data_receiver_keyvault.result
}
