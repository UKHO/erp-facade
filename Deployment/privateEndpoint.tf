
data "azurerm_resource_group" "perg" {
    provider = azurerm.erp
    name = var.spoke_rg
}

data "azurerm_virtual_network" "pevn" {
    provider = azurerm.erp
    name = var.pe_vnet_name
    resource_group_name = var.spoke_rg
}

data "azurerm_subnet" "pesn" {
    provider = azurerm.erp
    name = var.pe_subnet_name
    virtual_network_name = var.pe_vnet_name
    resource_group_name = var.spoke_rg
}

module "private_endpoint_link" {
  source              = "github.com/UKHO/tfmodule-azure-private-endpoint-private-link?ref=0.7.1"
  providers = {
    azurerm.hub   = azurerm.hub
    azurerm.spoke   = azurerm.erp
  }
  private_connection  = var.deploy_adds_mocks ? [local.private_connection, local.mock_private_connection] : [local.private_connection]
  zone_group          = local.zone_group 
  pe_identity         = var.deploy_adds_mocks ? [local.pe_identity, local.mock_pe_identity] : [local.pe_identity]
  pe_environment      = local.env_name 
  pe_vnet_rg          = var.spoke_rg 
  pe_vnet_name        = var.pe_vnet_name
  pe_subnet_name      = var.pe_subnet_name
  pe_resource_group   = var.deploy_adds_mocks ? [azurerm_resource_group.rg.name, azurerm_resource_group.rg.name] : [azurerm_resource_group.rg.name]
  dns_resource_group  = local.dns_resource_group
  pe_resource_group_locations = var.deploy_adds_mocks ? [azurerm_resource_group.rg.location, azurerm_resource_group.rg.location] : [azurerm_resource_group.rg.location]
}

