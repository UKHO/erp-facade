resource "azurerm_storage_account" "storage" {
  name                               = var.name
  resource_group_name                = var.resource_group_name
  location                           = var.location
  account_tier                       = "Standard"
  account_replication_type           = "LRS"
  account_kind                       = "StorageV2"
  allow_nested_items_to_be_public    = false
  public_network_access_enabled      = true
  tags                               = var.tags
}

resource "azurerm_storage_container" "erp_facade_container" {
  name                      = var.container_name
  storage_account_name      = azurerm_storage_account.storage.name
}

resource "azurerm_role_assignment" "storage_data_contributor_role" {
  scope                = azurerm_storage_account.storage.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.webapp_principal_id
}

resource "azurerm_role_assignment" "storage_data_contributor_role_slot" {
  scope                = azurerm_storage_account.storage.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.slot_principal_id
}
