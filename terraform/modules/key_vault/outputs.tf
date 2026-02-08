output "id" {
  description = "Resource ID of the Key Vault"
  value       = azurerm_key_vault.this.id
}

output "vault_uri" {
  description = "Vault URI"
  value       = azurerm_key_vault.this.vault_uri
}

output "name" {
  description = "Key Vault name"
  value       = azurerm_key_vault.this.name
}

output "resource_group_name" {
  description = "Resource group name"
  value       = azurerm_key_vault.this.resource_group_name
}

output "location" {
  description = "Location of the Key Vault"
  value       = azurerm_key_vault.this.location
}
