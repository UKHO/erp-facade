terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=3.50.0"
    }
  }

  required_version = "=1.4.4"
  backend "azurerm" {
    container_name = "tfstate"
    key            = "terraform.deployment.tfplan"
  }
}

provider "azurerm" {
  features {}
}