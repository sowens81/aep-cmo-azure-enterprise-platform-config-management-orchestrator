resource "random_uuid" "cc_role_kv" {
  keepers = {
    principal = module.function.identity_principal_id
    scope     = module.key_vault.id
  }
}

resource "azurerm_role_assignment" "cc_kv_secrets_officer" {
  scope              = module.key_vault.id
  role_definition_id = data.azurerm_role_definition.kv_secrets_officer.id
  principal_id       = module.function.identity_principal_id
  name               = random_uuid.cc_role_kv.result
}

resource "random_uuid" "cc_role_appconfig" {
  keepers = {
    principal = module.function.identity_principal_id
    scope     = module.app_configuration.id
  }
}

resource "azurerm_role_assignment" "cc_appconfig_data_owner" {
  scope              = module.app_configuration.id
  role_definition_id = data.azurerm_role_definition.appconfig_data_owner.id
  principal_id       = module.function.identity_principal_id
  name               = random_uuid.cc_role_appconfig.result
}