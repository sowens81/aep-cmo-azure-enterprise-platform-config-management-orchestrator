output "function_app" {
  description = "Outputs from the Function App module"
  value       = var.local_development ? null : module.function[0]
}

output "identity_id" {
  description = "User assigned identity ID for the Function App"
  value = module.function
}

output "app_configuration" {
  description = "Outputs from the App Configuration module"
  value = module.app_configuration
}

output "key_vault" {
  description = "Outputs from the Key Vault module"
  value = module.key_vault
}

output "service_bus_subscriptions" {
  description = "List of Service Bus subscription resources created for the environment"
  value = concat([module.servicebus_subscription_app_config_sync], [module.servicebus_subscription_key_vault_sync])
}
