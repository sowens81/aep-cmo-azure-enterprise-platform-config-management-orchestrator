# Assign least-privilege RBAC on spoke resources to the module-created identity
resource "random_uuid" "mi_role_appconfig" {
  count = var.local_development ? 0 : 1
  keepers = {
    principal = module.function[0].identity_principal_id
    scope     = module.app_configuration.id
  }
}

resource "azurerm_role_assignment" "mi_appconfig_data_owner" {
  count              = var.local_development ? 0 : 1
  scope              = module.app_configuration.id
  role_definition_id = data.azurerm_role_definition.appconfig_data_owner.id
  principal_id       = module.function[0].identity_principal_id
  name               = random_uuid.mi_role_appconfig[0].result
}

resource "random_uuid" "mi_role_kv" {
  count = var.local_development ? 0 : 1
  keepers = {
    principal = module.function[0].identity_principal_id
    scope     = module.key_vault.id
  }
}

resource "azurerm_role_assignment" "mi_kv_secrets_officer" {
  count              = var.local_development ? 0 : 1
  scope              = module.key_vault.id
  role_definition_id = data.azurerm_role_definition.kv_secrets_officer.id
  principal_id       = module.function[0].identity_principal_id
  name               = random_uuid.mi_role_kv[0].result
}

resource "azuread_group_member" "this" {
  count            = var.local_development ? 0 : 1
  group_object_id  = azuread_group.this.object_id
  member_object_id = module.function[0].identity_principal_id
  depends_on       = [azuread_group.this]
}
