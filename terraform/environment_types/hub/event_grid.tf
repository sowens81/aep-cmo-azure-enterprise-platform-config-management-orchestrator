# Event Grid - App Configuration
resource "azurerm_eventgrid_system_topic" "appconfig_shared_to_servicebus" {
  name                = "appconfig-shared-to-config-sync-system-topic"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  source_resource_id  = module.app_configuration.id
  topic_type          = "Microsoft.AppConfiguration.ConfigurationStores"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.eventgrid.id]
  }
}

resource "azurerm_eventgrid_system_topic_event_subscription" "appconfig_shared_to_servicebus" {
  name                = "appconfig-to-servicebus-event-topic"
  system_topic        = azurerm_eventgrid_system_topic.appconfig_shared_to_servicebus.name
  resource_group_name = azurerm_resource_group.this.name

  # Event types from App Configuration
  included_event_types = [
    "Microsoft.AppConfiguration.KeyValueModified",
    "Microsoft.AppConfiguration.KeyValueDeleted"
  ]

  # TODO: Add Storage Account for dead-lettering Qeueue and Retry Policy
  advanced_filter {
    string_contains {
      key    = "data.label"
      values = ["SYNC_SPOKE"]
    }
  }

  advanced_filtering_on_arrays_enabled = true

  # Send to Service Bus Topic
  service_bus_topic_endpoint_id = module.servicebus.topics[var.servicebus_config.app_config_event_topic_name]

  retry_policy {
    max_delivery_attempts = 10
    event_time_to_live    = 1440 # minutes (24h)
  }

  delivery_identity {
    type                   = "UserAssigned"
    user_assigned_identity = azurerm_user_assigned_identity.eventgrid.id
  }

  depends_on = [
    module.servicebus,
    module.app_configuration,
    azurerm_user_assigned_identity.eventgrid,
    azurerm_role_assignment.eg_mi_appconfig_data_reader,
    azurerm_role_assignment.eg_mi_servicebus_data_sender
  ]
}

# Event Grid - Key Vault
resource "azurerm_eventgrid_system_topic" "key_vault_shared_to_servicebus" {
  name                = "key-vault-shared-to-config-sync-system-topic"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  source_resource_id  = module.key_vault.id
  topic_type          = "Microsoft.KeyVault.vaults"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.eventgrid.id]
  }
}

resource "azurerm_eventgrid_system_topic_event_subscription" "key_vault_shared_to_servicebus" {
  name                = "key-vault-to-servicebus-event-topic"
  system_topic        = azurerm_eventgrid_system_topic.key_vault_shared_to_servicebus.name
  resource_group_name = azurerm_resource_group.this.name

  # Event types from Key Vault
  included_event_types = [
    "Microsoft.KeyVault.SecretNewVersionCreated"
  ]

  # TODO: Add Storage Account for dead-lettering Queue and Retry Policy

  # Send to Service Bus Topic
  service_bus_topic_endpoint_id = module.servicebus.topics[var.servicebus_config.key_vault_event_topic_name]

  retry_policy {
    max_delivery_attempts = 10
    event_time_to_live    = 1440 # minutes (24h)
  }

  delivery_identity {
    type                   = "UserAssigned"
    user_assigned_identity = azurerm_user_assigned_identity.eventgrid.id
  }

  depends_on = [
    module.servicebus,
    module.key_vault,
    azurerm_user_assigned_identity.eventgrid,
    azurerm_role_assignment.eg_mi_kv_secrets_reader,
    azurerm_role_assignment.eg_mi_servicebus_data_sender
  ]
}