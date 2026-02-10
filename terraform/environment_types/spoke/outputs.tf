output "function_app" {
  value = module.function
}

output "identity_id" {
  value = module.function
}

output "app_configuration" {
  value = module.app_configuration
}

output "key_vault" {
  value = module.key_vault
}

output "service_bus_subscriptions" {
  description = "List of Service Bus subscription resources created for the environment"
  value = concat([module.servicebus_subscription_app_config_sync], [module.servicebus_subscription_key_vault_sync])
}
