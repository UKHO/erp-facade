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
  key_vault_name     = "${local.service_name}-ukho-${local.env_name}-kv"
    SERVICE                   = "ERP Facade"
    ENVIRONMENT               = local.env_name
    SERVICE_OWNER             = "UKHO"
    RESPONSIBLE_TEAM          = "Mastek"
    CALLOUT_TEAM              = "On-Call_N/A"
    COST_CENTRE               = "011.05.12"
  }

variable "sku_name" {
  type = map(any)
  default = {
             "dev"  =  "P1v2"
             "qa"   =  "P1v3"
            }
}

