provider "azurerm" {
  features {}
}

locals {
  
}

provider "azurerm" {
  features {} 
  alias = "hub"
  subscription_id = local.hub_subscription_id
}

provider "azurerm" {
  features {} 
  alias = "erp"
  subscription_id = var.subscription_id
}