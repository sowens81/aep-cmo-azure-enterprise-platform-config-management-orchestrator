output "id" {
  description = "Resource ID of the App Configuration instance"
  value       = azurerm_app_configuration.this.id
}

output "endpoint" {
  description = "Primary endpoint URI for the App Configuration instance"
  value       = azurerm_app_configuration.this.endpoint
}

output "name" {
  description = "Name of the App Configuration instance"
  value       = azurerm_app_configuration.this.name
}

output "resource_group_name" {
  description = "Resource group name"
  value       = azurerm_app_configuration.this.resource_group_name
}

output "location" {
  description = "Location of the resource"
  value       = azurerm_app_configuration.this.location
}
