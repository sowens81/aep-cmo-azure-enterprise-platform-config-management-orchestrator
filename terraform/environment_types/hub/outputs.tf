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