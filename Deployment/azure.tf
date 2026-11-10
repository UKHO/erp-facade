terraform {
  backend "azurerm" {
    key                  = "terraform.deployment.tfplan"
    container_name       = "tfstate"
    version              = ">= 4.54.0"
  }
}

provider "azurerm" {
  features {}
  version              = ">= 4.54.0"
}

provider "azurerm" {
  features {} 
  alias = "hub"
  subscription_id = var.hub_subscription_id
  version              = ">= 4.54.0"
}

provider "azurerm" {
  features {} 
  alias = "erp"
  subscription_id = var.subscription_id
  version              = ">= 4.54.0"
}

provider "azurerm" {
  features {}
  alias = "frontdoor"
  # Only target the Front Door subscription when enabled, so disabled envs never touch it.
  subscription_id = var.front_door_enabled && var.front_door_subscription_id != "" ? var.front_door_subscription_id : null
}
