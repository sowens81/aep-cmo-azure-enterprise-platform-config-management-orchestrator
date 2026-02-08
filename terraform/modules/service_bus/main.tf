resource "azurerm_servicebus_namespace" "this" {
  name                = var.namespace_name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.sku
  capacity            = var.capacity
  tags                = var.tags
}

resource "azurerm_servicebus_topic" "this" {
  for_each = var.topics

  name         = each.key
  namespace_id = azurerm_servicebus_namespace.this.id

  partitioning_enabled = lookup(each.value, "enable_partitioning", false)
  express_enabled      = lookup(each.value, "enable_express", false)
  default_message_ttl  = lookup(each.value, "default_message_time_to_live", null)
}
