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

module "servicebus" {
  source = "../../modules/service_bus"

  namespace_name      = var.servicebus_config.namespace_name
  resource_group_name = var.servicebus_config.resource_group_name
  location            = var.location

  topics = {
    "${var.servicebus_config.sync_topic_name}" = {
      enable_partitioning          = false
      enable_express               = false
      default_message_time_to_live = null
    }
    "${var.servicebus_config.result_topic_name}" = {
      enable_partitioning          = false
      enable_express               = false
      default_message_time_to_live = null
    }
  }
}

data "azurerm_role_definition" "appconfig_data_owner" {
  name  = "App Configuration Data Owner"
  scope = data.azurerm_client_config.current.subscription_id
}

data "azurerm_role_definition" "appconfig_data_reader" {
  name  = "App Configuration Data Reader"
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

data "azurerm_role_definition" "sbus_data_sender" {
  name  = "Azure Service Bus Data Sender"
  scope = data.azurerm_client_config.current.subscription_id
}

data "azurerm_role_definition" "sbus_data_receiver" {
  name  = "Azure Service Bus Data Receiver"
  scope = data.azurerm_client_config.current.subscription_id
}

resource "azuread_group" "this" {
  display_name     = "so-demo-spoke-config-management-consumers"
  owners           = [data.azurerm_client_config.current.object_id]
  security_enabled = true
}

resource "azurerm_user_assigned_identity" "eventgrid" {
  name                = "eventgrid-to-servicebus-identity"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location

}

# Assign least-privilege RBAC on spoke resources to the module-created identity

# Current Context RBAC
## Key Vault Secrets Officer
resource "random_uuid" "cc_role_kv" {
  keepers = {
    principal = data.azurerm_client_config.current.object_id
    scope     = module.key_vault.id
  }
}

resource "azurerm_role_assignment" "cc_kv_secrets_officer" {
  scope              = module.key_vault.id
  role_definition_id = data.azurerm_role_definition.kv_secrets_officer.id
  principal_id       = data.azurerm_client_config.current.object_id
  name               = random_uuid.cc_role_kv.result
}

# Managed Identity RBAC
## App Configuration Data Reader
resource "random_uuid" "mi_role_appconfig" {
  keepers = {
    principal = azurerm_user_assigned_identity.eventgrid.principal_id
    scope     = module.app_configuration.id
  }
}

resource "azurerm_role_assignment" "mi_appconfig_data_reader" {
  scope              = module.app_configuration.id
  role_definition_id = data.azurerm_role_definition.appconfig_data_reader.id
  principal_id       = azurerm_user_assigned_identity.eventgrid.principal_id
  name               = random_uuid.mi_role_appconfig.result
}

## App Configuration Data Sender
resource "random_uuid" "mi_servicebus_data_sender" {
  keepers = {
    principal = azurerm_user_assigned_identity.eventgrid.principal_id
    scope     = module.servicebus.topics[var.servicebus_config.sync_topic_name].id
  }
}

resource "azurerm_role_assignment" "mi_servicebus_data_sender" {
  scope              = module.servicebus.topics[var.servicebus_config.sync_topic_name].id
  role_definition_id = data.azurerm_role_definition.sbus_data_sender.id
  principal_id       = azurerm_user_assigned_identity.eventgrid.principal_id
  name               = random_uuid.mi_servicebus_data_sender.result
}

# Entraid Group RBAC
## Key Vault Secrets Reader
resource "random_uuid" "entraid_group_role_kv" {
  keepers = {
    principal = azuread_group.this.object_id
    scope     = module.key_vault.id
  }
}

resource "azurerm_role_assignment" "entraid_group_kv_secrets_reader" {
  scope              = module.key_vault.id
  role_definition_id = data.azurerm_role_definition.kv_secrets_reader.id
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

## Service Bus Data Receiver
resource "random_uuid" "entraid_group_role_servicebus_data_receiver" {
  keepers = {
    principal = azuread_group.this.object_id
    scope     = module.servicebus.topics[var.servicebus_config.sync_topic_name].id
  }
}

resource "azurerm_role_assignment" "entraid_group_servicebus_data_receiver" {
  scope              = module.servicebus.topics[var.servicebus_config.sync_topic_name].id
  role_definition_id = data.azurerm_role_definition.sbus_data_receiver.id
  principal_id       = azuread_group.this.object_id
  name               = random_uuid.entraid_group_role_servicebus_data_receiver.result
}

## Service Bus Data Sender
resource "random_uuid" "entraid_group_role_servicebus_data_sender" {
  keepers = {
    principal = azuread_group.this.object_id
    scope     = module.servicebus.topics[var.servicebus_config.result_topic_name].id
  }
}

resource "azurerm_role_assignment" "entraid_group_servicebus_data_sender" {
  scope              = module.servicebus.topics[var.servicebus_config.result_topic_name].id
  role_definition_id = data.azurerm_role_definition.sbus_data_sender.id
  principal_id       = azuread_group.this.object_id
  name               = random_uuid.entraid_group_role_servicebus_data_sender.result
}

resource "azurerm_eventgrid_event_subscription" "appconfig_shared_to_servicebus" {
  name  = "appconfig-shared-to-config-sync"
  scope = module.app_configuration.id

  # Event types from App Configuration
  included_event_types = [
    "Microsoft.AppConfiguration.KeyValueModified",
    "Microsoft.AppConfiguration.KeyValueDeleted"
  ]


  advanced_filter {
    string_contains {
      key   = "data.keyLabel"
      values = [ "shared" ]
    }
  }

  # Send to Service Bus Topic
  service_bus_topic_endpoint_id = module.servicebus.topics[var.servicebus_config.sync_topic_name].id

  retry_policy {
    max_delivery_attempts = 10
    event_time_to_live    = 1440 # minutes (24h)
  }

  delivery_identity {
    type                   = "UserAssigned"
    user_assigned_identity = azurerm_user_assigned_identity.eventgrid.id
  }
}
