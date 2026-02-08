provider "azurerm" {
  subscription_id = var.environment_subscription_id
  features {
  }
}

provider "random" {}
