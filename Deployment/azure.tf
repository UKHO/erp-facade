terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=3.89.0"
      configuration_aliases = [ # merged from pvt ep module
        azurerm.hub,
        azurerm.spoke,
      ]
    }
  }

  required_version = "=1.7.2"
  backend "azurerm" {
    container_name = "tfstate"
    key            = "terraform.deployment.tfplan"
  }
}

locals {
  hub_subscription_id = "282900b8-5415-4137-afcc-fd13fe9a64a7"
}

provider "azurerm" {
  features {} 
  alias = "hub"
  subscription_id = local.hub_subscription_id
}
