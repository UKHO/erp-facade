variable "location" {
  type    = string
  default = "uksouth"
}

variable "resource_group_name" {
  type    = string
  default = "erpfacade"
}

variable "subscription_id" {
  type = string
}

variable "hub_subscription_id" {
  type = string
}

variable "dns_zone_rg" {
  type = string  
}

variable "sku_name" {
  type = map(any)
  default = {
            "dev"     =  "P1v2"            
            "vni"     =  "P1v3"
            "iat"     =  "P1v3"
            "prp"     =  "P1v3"     
            "vne"     =  "P1v3"
            "qa"      =  "P1v3"
            live      =  "P1v3"
            }
}

variable "spoke_rg" {
  type = string
}

variable "pe_rg" {
  type = string
}

variable "spoke_vnet_name" {
  type = string
}

variable "spoke_subnet_name" {
  type = string
}

variable "pe_vnet_name" {
  type = string
}

variable "pe_subnet_name" {
  type = string
}

variable "deploy_adds_mocks" {
  type = string  
}
