locals {
  stubs = (
    local.env_name =="vne" ? [0] : []
  )
}

import { 
    for_each = local.stubs
    to = module.private_endpoint_link.azurerm_private_endpoint.main
    id = "/subscriptions/a3b2d2c4-28e8-4e1c-98c3-9804de5561b4/resourceGroups/erpfacade-vne-rg/providers/Microsoft.Network/privateEndpoints/m-erpvne2sap-vne-pe"
}

import { 
    for_each = local.stubs
    to = module.private_endpoint_link.azurerm_private_dns_zone_virtual_network_link.main
    id = "/subscriptions/282900b8-5415-4137-afcc-fd13fe9a64a7/resourceGroups/engineering-rg/providers/Microsoft.Network/privateDnsZones/privatelink.azurewebsites.net/virtualNetworkLinks/erpe2e2sap"
}
