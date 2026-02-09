module "app_configuration" {
  source              = "../../modules/app_configuration"
  name                = var.app_configuration_name
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku                 = var.app_configuration_sku
  tags                = var.tags
}