resource "azurerm_storage_account" "storage" {
  name                               = var.name
  resource_group_name                = var.resource_group_name
  location                           = var.location
  account_tier                       = "Standard"
  account_replication_type           = "LRS"
  account_kind                       = "StorageV2"
  allow_nested_items_to_be_public    = false
  tags                               = var.tags
}
resource "azurerm_storage_container" "erp_facade-cotainer" {
  name                      = var.container_name
  storage_account_name      = azurerm_storage_account.storage.name
}

resource "azurerm_storage_table" "banner_notification_table" {
  name                 = var.table_name
  storage_account_name = azurerm_storage_account.cache_storage.name
}

resource "azurerm_role_assignment" "storage_data_contributor_role" {
  scope                = azurerm_storage_account.storage.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.webapp_principal_id
}