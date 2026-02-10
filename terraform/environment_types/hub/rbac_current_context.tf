# Current Context RBAC
## Key Vault Secrets Officer
resource "random_uuid" "cc_hub_role_kv" {
  keepers = {
    principal = data.azurerm_client_config.current.object_id
    scope     = module.key_vault.id
  }
}

resource "azurerm_role_assignment" "cc_hub_kv_secrets_officer" {
  scope              = module.key_vault.id
  role_definition_id = data.azurerm_role_definition.kv_secrets_officer.id
  principal_id       = data.azurerm_client_config.current.object_id
  name               = random_uuid.cc_hub_role_kv.result
}

resource "random_uuid" "cc_hub_role_sbus" {
  keepers = {
    principal = data.azurerm_client_config.current.object_id
    scope     = module.servicebus.namespace_id
  }
}

resource "azurerm_role_assignment" "cc_hub_sbus_data_owner" {
  scope              = module.servicebus.namespace_id
  role_definition_id = data.azurerm_role_definition.sbus_data_owner.id
  principal_id       = data.azurerm_client_config.current.object_id
  name               = random_uuid.cc_hub_role_sbus.result
}


