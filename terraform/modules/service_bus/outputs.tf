output "namespace_id" {
  description = "The id of the Service Bus namespace"
  value       = azurerm_servicebus_namespace.this.id
}

output "namespace_name" {
  description = "The name of the Service Bus namespace"
  value       = azurerm_servicebus_namespace.this.name
}

output "endpoint" {
  description = "The endpoint of the Service Bus namespace"
  value       = azurerm_servicebus_namespace.this.endpoint
  
}

output "topics" {
  description = "Map of topics created (name => id)"
  value       = { for k, v in azurerm_servicebus_topic.this : k => v.id }
}