data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "this" {
  name                       = var.name
  location                   = var.location
  resource_group_name        = var.resource_group_name
  tenant_id                  = length(var.tenant_id) > 0 ? var.tenant_id : data.azurerm_client_config.current.tenant_id
  sku_name                   = upper(var.sku) == "PREMIUM" ? "premium" : "standard"
  purge_protection_enabled   = var.purge_protection_enabled
  soft_delete_retention_days = var.soft_delete_retention_days > 0 ? var.soft_delete_retention_days : 7
  rbac_authorization_enabled = true

  tags = var.tags
}
