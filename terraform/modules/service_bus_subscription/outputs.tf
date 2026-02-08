output "subscription_id" {
  description = "The id of the created Service Bus subscription"
  value       = azurerm_servicebus_subscription.this.id
}

output "subscription_name" {
  description = "The name of the created Service Bus subscription"
  value       = azurerm_servicebus_subscription.this.name
}
