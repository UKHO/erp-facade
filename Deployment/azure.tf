terraform {
  backend "azurerm" {
    container_name       = "tfstate"
  }
}

provider "azurerm" {
  features {}
}

locals {
  
}

provider "azurerm" {
  features {} 
  alias = "hub"
  subscription_id = var.hub_subscription_id
}

provider "azurerm" {
  features {} 
  alias = "erp"
  subscription_id = var.subscription_id
}