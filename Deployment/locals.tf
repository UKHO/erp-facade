locals {
  env_name           = lower(terraform.workspace)
  service_name       = "erpfacade"  
  web_app_name       = "${local.service_name}-${local.env_name}-api"
  mock_web_app_name  = "${local.service_name}-${local.env_name}-sapmockservice"
  key_vault_name     = "${local.service_name}-ukho-${local.env_name}-kv"
  storage_name       = "${local.service_name}${local.env_name}storage"
  container_name     = "erp-container"
  perg = "erpfacade-${local.env_name}-rg"
  spokerg = "m-spokeconfig-rg"
  pe_identity = "erp${local.env_name}2sap"
  vnet_link = "erp${local.env_name}2sap"
  private_connection = "/subscriptions/${var.subscription_id}/resourceGroups/erpfacade-${local.env_name}-rg/providers/Microsoft.Web/sites/erpfacade-${local.env_name}-api"
  pe_subnet_name = "pe-subnet"
  pe_vnet_rg = "m-spokeconfig-rg"
  dns_resource_group = "engineering-rg"
  zone_group = "erp${local.env_name}2sapzone"
  dns_zones = "privatelink.azurewebsites.net"       
  tags = {
    SERVICE                   = "ERP Facade"
    ENVIRONMENT               = local.env_name
    SERVICE_OWNER             = "UKHO"
    RESPONSIBLE_TEAM          = "Abzu"
    CALLOUT_TEAM              = "On-Call_N/A"
    COST_CENTRE               = "011.05.12"
    }
}

