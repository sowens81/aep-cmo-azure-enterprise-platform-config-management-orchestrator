# Assign least-privilege RBAC on spoke resources to the module-created identity
resource "random_uuid" "mi_role_appconfig" {
  keepers = {
    principal = module.function.identity_principal_id
    scope     = module.app_configuration.id
  }
}

resource "azurerm_role_assignment" "mi_appconfig_data_owner" {
  scope              = module.app_configuration.id
  role_definition_id = data.azurerm_role_definition.appconfig_data_owner.id
  principal_id       = module.function.identity_principal_id
  name               = random_uuid.mi_role_appconfig.result
}

resource "random_uuid" "mi_role_kv" {
  keepers = {
    principal = module.function.identity_principal_id
    scope     = module.key_vault.id
  }
}

resource "azurerm_role_assignment" "mi_kv_secrets_officer" {
  scope              = module.key_vault.id
  role_definition_id = data.azurerm_role_definition.kv_secrets_officer.id
  principal_id       = module.function.identity_principal_id
  name               = random_uuid.mi_role_kv.result
}

resource "azuread_group_member" "this" {
  group_object_id  = data.azuread_group.this.object_id
  member_object_id = module.function.identity_principal_id
}
