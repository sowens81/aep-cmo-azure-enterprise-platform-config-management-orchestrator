output "function_app" {
  description = "Outputs from the Function App module"
  value       = var.local_development ? null : module.function[0]
}

output "app_configuration" {
  description = "Outputs from the App Configuration module"
  value       = module.app_configuration
}

output "key_vault" {
  description = "Outputs from the Key Vault module"
  value       = module.key_vault
}

output "servicebus" {
  description = "Outputs from the Service Bus module"
  value       = module.servicebus
}

output "entraid_group_hub_access" {
  description = "Outputs from the Entra ID Group for Hub Access module"
  value       = azuread_group.this
}

output "service_bus_subscriptions" {
  description = "List of Service Bus subscription resources created for the environment"
  value       = concat([module.servicebus_subscription_app_config_event], [module.servicebus_subscription_key_vault_event])
}