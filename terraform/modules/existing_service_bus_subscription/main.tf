data "azurerm_servicebus_namespace" "this" {
  name                = var.servicebus_namespace_name
  resource_group_name = var.servicebus_namespace_resource_group_name
}

data "azurerm_servicebus_topic" "this" {
  name         = var.topic_name
  namespace_id = data.azurerm_servicebus_namespace.this.id
}

resource "azurerm_servicebus_subscription" "this" {

  name     = var.subscription_name
  topic_id = data.azurerm_servicebus_topic.this.id

  lock_duration = var.lock_duration

  # Clamp max_delivery_count to the requested 5..10 range
  max_delivery_count = max(5, min(var.max_delivery_count, 10))

  dead_lettering_on_message_expiration      = var.enable_dead_lettering
  dead_lettering_on_filter_evaluation_error = var.enable_dead_lettering

  default_message_ttl = var.default_message_ttl
}
