data "azurerm_client_config" "current" {}

resource "azurerm_resource_group" "this" {
  name     = var.resource_group_name
  location = var.location
}


module "app_configuration" {
  source              = "../../modules/app_configuration"
  name                = var.app_configuration_name
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku                 = var.app_configuration_sku
  tags                = var.tags
}

module "key_vault" {
  source              = "../../modules/key_vault"
  name                = var.key_vault_name
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  tenant_id           = data.azurerm_client_config.current.tenant_id
  tags                = var.tags
}

# Lookup built-in role definitions to avoid relying on localized role names
data "azurerm_role_definition" "appconfig_data_owner" {
  name  = "App Configuration Data Owner"
  scope = data.azurerm_client_config.current.subscription_id
}

data "azurerm_role_definition" "kv_secrets_officer" {
  name  = "Key Vault Secrets Officer"
  scope = data.azurerm_client_config.current.subscription_id
}

data "azurerm_role_definition" "kv_secrets_reader" {
  name  = "Key Vault Secrets Reader"
  scope = data.azurerm_client_config.current.subscription_id
}

module "function" {
  source               = "../../modules/linux_function_app"
  name                 = var.function_name
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  identity_name        = var.identity_name
  storage_account_name = var.storage_account_name
  app_settings = {
    APP_CONFIG_ENDPOINT          = module.app_configuration.endpoint
    SBUS_NAMESPACE_ENDPOINT      = "sb://${var.servicebus_config.namespace_name}.servicebus.windows.net/"
    SBUS_TOPIC_SYNC              = "${var.servicebus_config.sync_topic_name}"
    SBUS_TOPIC_SYNC_SUBSCRIPTION = "${module.servicebus_subscription_sync.subscription_name}"
    SBUS_TOPIC_SYNC_RESULT       = "${var.servicebus_config.result_topic_name}"
    KEY_VAULT_URI                = module.key_vault.vault_uri
    HUB_KEY_VAULT_URI            = var.hub_key_vault_uri
    IDEMPOTENCY_TABLE_NAME       = var.idempotency_table
  }
}

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

module "servicebus_subscription_sync" {
  source = "../../modules/service_bus_subscription"

  providers = { azurerm = azurerm.hub }
  servicebus_namespace_name                = var.servicebus_config.namespace_name
  servicebus_namespace_resource_group_name = var.servicebus_config.resource_group_name
  topic_name                               = var.servicebus_config.sync_topic_name
  subscription_name                        = "${var.servicebus_config.sync_topic_name}/${var.organisation}/${var.environment}"
}

data "azuread_group" "this" {
  display_name     = "so-demo-spoke-config-management-consumers"
  security_enabled = true
}

resource "azuread_group_member" "this" {
  group_object_id  = data.azuread_group.this.object_id
  member_object_id = module.function.identity_principal_id
}
