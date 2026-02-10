module "servicebus" {
  source = "../../modules/service_bus"

  namespace_name      = var.servicebus_config.namespace_name
  resource_group_name = azurerm_resource_group.this.name
  location            = var.location
  capacity            = 0

  topics = {
    "{var.servicebus_config.result_topic_name}" = {
      enable_partitioning          = false
      enable_express               = false
      default_message_time_to_live = null
    }
    "{var.servicebus_config.app_config_sync_topic_name}" = {
      enable_partitioning          = false
      enable_express               = false
      default_message_time_to_live = null
    }
    "{var.servicebus_config.app_config_event_topic_name}" = {
      enable_partitioning          = false
      enable_express               = false
      default_message_time_to_live = null
    }
    "{var.servicebus_config.key_vault_sync_topic_name}" = {
      enable_partitioning          = false
      enable_express               = false
      default_message_time_to_live = null
    }
    "{var.servicebus_config.key_vault_event_topic_name}" = {
      enable_partitioning          = false
      enable_express               = false
      default_message_time_to_live = null
    }
  }
}
