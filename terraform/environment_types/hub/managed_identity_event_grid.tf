# Event Grid Managed Identity RBAC
resource "azurerm_user_assigned_identity" "eventgrid" {
  name                = "eventgrid-to-servicebus-identity"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location

}

## App Configuration Data Reader
resource "random_uuid" "eg_mi_role_appconfig" {
  keepers = {
    principal = azurerm_user_assigned_identity.eventgrid.principal_id
    scope     = module.app_configuration.id
  }
}

resource "azurerm_role_assignment" "eg_mi_appconfig_data_reader" {
  scope              = module.app_configuration.id
  role_definition_id = data.azurerm_role_definition.appconfig_data_reader.id
  principal_id       = azurerm_user_assigned_identity.eventgrid.principal_id
  name               = random_uuid.eg_mi_role_appconfig.result
}

## Key Vault Secrets User
resource "random_uuid" "eg_mi_kv" {
  keepers = {
    principal = azurerm_user_assigned_identity.eventgrid.principal_id
    scope     = module.key_vault.id
  }
}

resource "azurerm_role_assignment" "eg_mi_kv_secrets_user" {
  scope              = module.key_vault.id
  role_definition_id = data.azurerm_role_definition.kv_secrets_user.id
  principal_id       = azurerm_user_assigned_identity.eventgrid.principal_id
  name               = random_uuid.eg_mi_kv.result
}

## Azure Service Bus Data Sender
resource "random_uuid" "eg_mi_servicebus_data_sender" {
  keepers = {
    principal = azurerm_user_assigned_identity.eventgrid.principal_id
    scope     = module.servicebus.namespace_id
  }
}

resource "azurerm_role_assignment" "eg_mi_servicebus_data_sender" {
  scope              = module.servicebus.namespace_id
  role_definition_id = data.azurerm_role_definition.sbus_data_sender.id
  principal_id       = azurerm_user_assigned_identity.eventgrid.principal_id
  name               = random_uuid.eg_mi_servicebus_data_sender.result
}