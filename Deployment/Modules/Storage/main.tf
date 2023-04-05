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
  name                      = "erp-container"
  storage_account_name      = azurerm_storage_account.storage.name
}
