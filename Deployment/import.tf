import {
  to = module.key_vault.azurerm_key_vault.kv
  id = "${azurerm_resource_group.rg.id}/providers/Microsoft.KeyVault/vaults/${local.key_vault_name}"  
}