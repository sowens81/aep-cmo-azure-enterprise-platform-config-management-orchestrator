output "function_app_id" {
  description = "ID of the Function App"
  value       = azurerm_function_app_flex_consumption.this.id
}

output "function_app_default_hostname" {
  description = "Default hostname for the Function App"
  value       = azurerm_function_app_flex_consumption.this.default_hostname
}

output "identity_id" {
  description = "User Assigned Identity resource id"
  value       = azurerm_user_assigned_identity.this.id
}

output "identity_principal_id" {
  description = "Principal ID of the user assigned identity"
  value       = azurerm_user_assigned_identity.this.principal_id
}

output "storage_account_id" {
  description = "Storage account id"
  value       = azurerm_storage_account.this.id
}
