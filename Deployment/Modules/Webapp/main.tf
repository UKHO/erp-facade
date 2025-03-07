resource "azurerm_service_plan" "app_service_plan" {
  name                = "${var.service_name}-asp"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku_name            = var.sku_name
  os_type             = "Windows"
  tags                = var.tags
}

resource "azurerm_windows_web_app" "webapp_service" {
  name                = var.name
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = azurerm_service_plan.app_service_plan.id
  tags                = var.tags

  site_config {
    application_stack {    
      current_stack  = "dotnet"
      dotnet_version = "v8.0"
    }
    always_on  = true
    ftps_state = "Disabled"
  }
     
  app_settings = var.app_settings

  sticky_settings {
    app_setting_names = [ "WEBJOBS_STOPPED" ]
  }

  identity {
    type = "SystemAssigned"
  }

  lifecycle {
    ignore_changes = [ virtual_network_subnet_id ]
  }

  https_only = true
}

resource "azurerm_windows_web_app_slot" "staging" {
  name                = "staging"
  app_service_id      = azurerm_windows_web_app.webapp_service.id
  tags                = azurerm_windows_web_app.webapp_service.tags 

  site_config {
    application_stack {    
      current_stack  = "dotnet"
      dotnet_version = "v8.0"
    }
    always_on  = true
    ftps_state = "Disabled"
  }
     
  app_settings = merge(azurerm_windows_web_app.webapp_service.app_settings, { "WEBJOBS_STOPPED" = "1" })

  identity {
    type = "SystemAssigned"
  }

  lifecycle {
    ignore_changes = [ virtual_network_subnet_id ]
  }

  https_only = azurerm_windows_web_app.webapp_service.https_only
}

resource "azurerm_windows_web_app" "mock_webapp_service" {
  count               = var.env_name == "dev" ? 1 : 0
  name                = var.mock_webapp_name
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = azurerm_service_plan.app_service_plan.id
  tags                = var.tags
  public_network_access_enabled      = false

  site_config {
    application_stack {    
      current_stack  = "dotnet"
      dotnet_version = "v8.0"
    }
    always_on  = true
    ftps_state = "Disabled"
  }
     
  app_settings = var.mock_app_settings

  identity {
    type = "SystemAssigned"
  }

  https_only = true
}

resource "azurerm_app_service_virtual_network_swift_connection" "webapp_vnet_integration" {
  app_service_id = azurerm_windows_web_app.webapp_service.id
  subnet_id      = var.subnet_id
}

resource "azurerm_app_service_virtual_network_swift_connection" "mock_webapp_vnet_integration" {
  count               = var.env_name == "dev" ? 1 : 0
  app_service_id = azurerm_windows_web_app.mock_webapp_service[0].id
  subnet_id      = var.subnet_id
}

resource "azurerm_app_service_slot_virtual_network_swift_connection" "slot_vnet_integration" {
  app_service_id = azurerm_windows_web_app.webapp_service.id
  subnet_id      = var.subnet_id
  slot_name      = azurerm_windows_web_app_slot.staging.name
}
