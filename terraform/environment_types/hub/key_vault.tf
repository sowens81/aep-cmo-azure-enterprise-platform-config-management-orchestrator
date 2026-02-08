module "key_vault" {
  source              = "../../modules/key_vault"
  name                = var.key_vault_name
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  tenant_id           = data.azurerm_client_config.current.tenant_id
  tags                = var.tags
}