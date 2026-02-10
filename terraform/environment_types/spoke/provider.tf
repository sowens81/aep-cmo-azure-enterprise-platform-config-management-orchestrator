provider "azurerm" {
  subscription_id = var.environment_subscription_id
  features {
  }
}

provider "azurerm" {
  alias           = "hub"
  subscription_id = var.hub_subscription_id
  features {
  }
}

provider "azuread" {
  tenant_id = var.tenant_id
}

provider "random" {}
