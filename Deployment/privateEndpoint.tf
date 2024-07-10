data "azurerm_resource_group" "rg" {
    provider = azurerm.erp
    name = local.perg
} 

data "azurerm_resource_group" "perg" {
    provider = azurerm.erp
    name = local.spokerg
}

data "azurerm_virtual_network" "pevn" {
    provider = azurerm.erp
    name = var.pe_vnet_name
    resource_group_name = local.spokerg
}

data "azurerm_subnet" "pesn" {
    provider = azurerm.erp
    name = local.pe_subnet_name
    virtual_network_name = var.pe_vnet_name
    resource_group_name = local.spokerg
}

module "private_endpoint_link" {
  source              = "github.com/UKHO/tfmodule-azure-private-endpoint-private-link?ref=0.6.0"
  providers = {
    azurerm.hub   = azurerm.hub
    azurerm.spoke   = azurerm.erp
  }
  vnet_link           = local.vnet_link
  private_connection  = [local.private_connection]
  zone_group          = local.zone_group 
  pe_identity         = [local.pe_identity] 
  pe_environment      = local.env_name 
  pe_vnet_rg          = local.pe_vnet_rg  
  pe_vnet_name        = var.pe_vnet_name
  pe_subnet_name      = local.pe_subnet_name
  pe_resource_group   = data.azurerm_resource_group.perg
  dns_resource_group  = local.dns_resource_group
}


