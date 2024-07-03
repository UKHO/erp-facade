data "azurerm_resource_group" "rg" {
    name = local.perg 
} 

data "azurerm_resource_group" "perg" {
    name = local.spokerg
}

data "azurerm_virtual_network" "pevn" {
    name = local.pvnet
    resource_group_name = local.spokerg
}

data "azurerm_subnet" "pesn" {
    name = local.pesn
    virtual_network_name = local.pvnet
    resource_group_name = local.spokerg
}

locals {
    perg = "erpfacade-e2e-rg"
    spokerg = "m-spokeconfig-rg"
    pvnet = "ERPFvNextE2E-vnet"
    pesn = "pe-subnet"  
    pe_environment = "e2e"
    pe_identity = "erpe2e2sap"
    vnet_link = "erpe2e2sap"
    private_connection = "/subscriptions/a3b2d2c4-28e8-4e1c-98c3-9804de5561b4/resourceGroups/erpfacade-e2e-rg/providers/Microsoft.Web/sites/erpfacade-e2e-api"
    pe_vnet_name = "ERPFvNextE2E-vnet"
    pe_subnet_name = "pe-subnet"
    pe_vnet_rg = "m-spokeconfig-rg"
    dns_resource_group = "engineering-rg"
    zone_group = "erpe2e2sapzone"
    dns_zones = "privatelink.azurewebsites.net"   
}

module "private_endpoint_link" {
  source              = "github.com/UKHO/tfmodule-azure-private-endpoint-private-link?ref=0.6.0"
  count               = var.sku_name == "e2e" ? 1 : 0 
  providers = {
    azurerm.src   = azurerm.hub
    azurerm.src   = azurerm.alias
  }
  vnet_link           = local.vnet_link
  private_connection  = [local.private_connection]
  zone_group          = local.zone_group 
  pe_identity         = [local.pe_identity] 
  pe_environment      = local.pe_environment 
  pe_vnet_rg          = local.pe_vnet_rg  
  pe_vnet_name        = local.pe_vnet_name
  pe_subnet_name      = local.pe_subnet_name
  pe_resource_group   = data.azurerm_resource_group.perg
  dns_resource_group  = local.dns_resource_group
}


