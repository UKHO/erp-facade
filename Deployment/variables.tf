variable "location" {
  type    = string
  default = "uksouth"
}

variable "resource_group_name" {
  type    = string
  default = "erpfacade"
}

locals {
  env_name           = lower(terraform.workspace)
  service_name       = "erpfacade"  
  web_app_name       = "${local.service_name}-${local.env_name}-api"
  mock_web_app_name  = "${local.service_name}-${local.env_name}-sapmockservice"
  key_vault_name     = "${local.service_name}-ukho-${local.env_name}-kv"
  storage_name       = "${local.service_name}${local.env_name}storage"
  container_name     = "erp-container"
  tags = {
    SERVICE                   = "ERP Facade"
    ENVIRONMENT               = local.env_name
    SERVICE_OWNER             = "UKHO"
    RESPONSIBLE_TEAM          = "Mastek"
    CALLOUT_TEAM              = "On-Call_N/A"
    COST_CENTRE               = "011.05.12"
    }
  }

variable "sku_name" {
  type = map(any)
  default = {
            "dev"     =  "P1v2"            
            "vni"     =  "P1v3"            
            "e2e"     =  "P1v3"
            "qa"      =  "P1v3"
            live      =  "P1v3"
            }
}

variable "spoke_rg" {
  type = string
}

variable "spoke_vnet_name" {
  type = string
}

variable "spoke_subnet_name" {
  type = string
}
