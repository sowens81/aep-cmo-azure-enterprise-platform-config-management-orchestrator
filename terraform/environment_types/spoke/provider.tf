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

provider "random" {}
