provider "azurerm" {
  features {}
}

locals {
  hub_subscription_id = "282900b8-5415-4137-afcc-fd13fe9a64a7"
  subscription_id = var.subscription_id
}

provider "azurerm" {
  features {} 
  alias = "hub"
  subscription_id = local.hub_subscription_id
}

provider "azurerm" {
  features {} 
  alias = "erpe2e"
  subscription_id = local.subscription_id
}