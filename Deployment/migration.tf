
import {
    count = local.env_name == "vne" ? 1 : 0
    to = module.private_endpoint_link.azurerm_private_endpoint.main
    id = "/subscriptions/a3b2d2c4-28e8-4e1c-98c3-9804de5561b4/resourceGroups/erpfacade-vne-rg/providers/Microsoft.Network/privateEndpoints/m-erpvne2sap-vne-pe"
}