data "azurerm_subnet" "main_subnet" {
  name                 = var.spoke_subnet_name
  virtual_network_name = var.spoke_vnet_name
  resource_group_name  = var.spoke_rg
}
  
module "app_insights" {
  source              = "./Modules/AppInsights"
  name                = "${local.service_name}-${local.env_name}-insights"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  tags                = local.tags
}

module "addsMock_app_insights" {
  count               = local.env_name == "iat" ? 1 : 0
  source              = "./Modules/AppInsights"
  name                = "${local.service_name}-${local.env_name}-sapmockinsights"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  tags                = local.tags
}
  
module "eventhub" {
  source              = "./Modules/EventHub"
  name                = "${local.service_name}-${local.env_name}-events"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  tags                = local.tags
  env_name            = local.env_name
}
  
module "webapp_service" {
  source                    = "./Modules/Webapp"
  name                      = local.web_app_name
  mock_webapp_name          = local.mock_web_app_name
  addsmock_webapp_name      = local.addsmock_webapp_name
  service_name              = local.service_name                 
  resource_group_name       = azurerm_resource_group.rg.name
  env_name                  = local.env_name
  location                  = azurerm_resource_group.rg.location
  sku_name                  = var.sku_name[local.env_name]
  subnet_id                 = data.azurerm_subnet.main_subnet.id  
  app_settings = {
    "KeyVaultSettings:ServiceUri"                              = "https://${local.key_vault_name}.vault.azure.net/"
    "EventHubLoggingConfiguration:Environment"                 = local.env_name
    "EventHubLoggingConfiguration:MinimumLoggingLevel"         = "Warning"
    "EventHubLoggingConfiguration:UkhoMinimumLoggingLevel"     = "Information"
    "APPINSIGHTS_INSTRUMENTATIONKEY"                           = module.app_insights.instrumentation_key
    "ASPNETCORE_ENVIRONMENT"                                   = local.env_name
    "WEBSITE_RUN_FROM_PACKAGE"                                 = "1"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                          = "true"
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG"          = "1"    
  }
  mock_app_settings = {
    "KeyVaultSettings:ServiceUri"                              = "https://${local.key_vault_name}.vault.azure.net/"
    "ASPNETCORE_ENVIRONMENT"                                   = local.env_name
    "WEBSITE_RUN_FROM_PACKAGE"                                 = "1"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                          = "true"
  }
  addsmock_app_settings = {   
    "ASPNETCORE_ENVIRONMENT"                                   = local.env_name
    "WEBSITE_RUN_FROM_PACKAGE"                                 = "1"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                          = "true"
    "APPINSIGHTS_INSTRUMENTATIONKEY"                           = module.addsMock_app_insights.instrumentation_key
  }
  tags                                                         = local.tags
}
  
locals {
  kv_read_access_list = {
    "webapp_service" = module.webapp_service.web_app_object_id
    "webapp_slot"    = module.webapp_service.slot_object_id
  }
  
  kv_read_access_list_with_mock = merge(local.kv_read_access_list, {
    "mock_service" = local.env_name == "dev" ? module.webapp_service.mock_web_app_object_id : ""
    })
}
  
module "key_vault" {
  source              = "./Modules/KeyVault"
  name                = local.key_vault_name
  resource_group_name = azurerm_resource_group.rg.name
  env_name            = local.env_name
  tenant_id           = module.webapp_service.web_app_tenant_id
  location            = azurerm_resource_group.rg.location
  read_access_objects = local.env_name == "dev" ? local.kv_read_access_list_with_mock : local.kv_read_access_list
  secrets = {
    "EventHubLoggingConfiguration--ConnectionString"            = module.eventhub.log_primary_connection_string
    "EventHubLoggingConfiguration--EntityPath"                  = module.eventhub.entity_path
    "ApplicationInsights--ConnectionString"                     = module.app_insights.connection_string
    "AzureStorageConfiguration--ConnectionString"               = module.storage.storage_connection_string
  }
  tags                                                          = local.tags
}
  
module "storage" {
  source                                = "./Modules/Storage"
  name                                  = local.storage_name
  resource_group_name                   = azurerm_resource_group.rg.name
  location                              = azurerm_resource_group.rg.location
  tags                                  = local.tags  
  container_name                        = local.container_name
  webapp_principal_id                   = module.webapp_service.web_app_object_id
  slot_principal_id                     = module.webapp_service.slot_object_id
}
