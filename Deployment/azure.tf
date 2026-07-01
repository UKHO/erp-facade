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
