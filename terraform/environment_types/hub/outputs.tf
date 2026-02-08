output "app_configuration_id" {
  value = module.app_configuration.id
}

output "key_vault_id" {
  value = module.key_vault.id
}

output "servicebus_topic_ids" {
  description = "Map of Service Bus topic names to their IDs"
  value       = module.servicebus.topics
}

