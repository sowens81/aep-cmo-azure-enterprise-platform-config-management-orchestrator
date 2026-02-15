module "servicebus_subscription_app_config_sync" {
  source = "../../modules/existing_service_bus_subscription"

  providers                                = { azurerm = azurerm.hub }
  servicebus_namespace_name                = var.servicebus_config.namespace_name
  servicebus_namespace_resource_group_name = var.servicebus_config.resource_group_name
  topic_name                               = var.servicebus_config.app_config_sync_topic_name
  subscription_name                        = "${var.servicebus_config.app_config_sync_topic_name}-${var.organisation}-${var.environment}"
}

module "servicebus_subscription_key_vault_sync" {
  source = "../../modules/existing_service_bus_subscription"

  providers                                = { azurerm = azurerm.hub }
  servicebus_namespace_name                = var.servicebus_config.namespace_name
  servicebus_namespace_resource_group_name = var.servicebus_config.resource_group_name
  topic_name                               = var.servicebus_config.key_vault_sync_topic_name
  subscription_name                        = "${var.servicebus_config.key_vault_sync_topic_name}-${var.organisation}-${var.environment}"
}