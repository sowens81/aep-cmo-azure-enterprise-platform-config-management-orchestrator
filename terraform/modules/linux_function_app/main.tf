resource "azurerm_user_assigned_identity" "this" {
  name                = var.identity_name
  resource_group_name = var.resource_group_name
  location            = var.location
}

resource "azurerm_storage_account" "this" {
  name                     = var.storage_account_name
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"
}


resource "azurerm_service_plan" "this" {
  name                = "${var.name}-plan"
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  sku_name            = var.app_service_plan_sku
}

resource "azurerm_linux_function_app" "this" {
  name                       = var.name
  location                   = var.location
  resource_group_name        = var.resource_group_name
  service_plan_id        = azurerm_service_plan.this.id
  storage_account_name       = azurerm_storage_account.this.name
  storage_account_access_key = azurerm_storage_account.this.primary_access_key
  functions_extension_version = var.function_version

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.this.id]
  }

  site_config {}

  app_settings = merge(var.app_settings, {
    FUNCTIONS_WORKER_RUNTIME  = var.worker_runtime
    WEBSITE_RUN_FROM_PACKAGE  = var.website_run_from_package ? "1" : "0"
    STORAGE_ACCOUNT_TABLE_URI = "https://${azurerm_storage_account.this.name}.table.core.windows.net"
    STORAGE_ACCOUNT_BLOB_URI  = "https://${azurerm_storage_account.this.name}.blob.core.windows.net"
  })

  tags = var.tags
}
