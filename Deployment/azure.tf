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

provider "azurerm" {
  features {}
}
