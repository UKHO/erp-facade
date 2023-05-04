output "web_app_object_id" {
  value = azurerm_windows_web_app.webapp_service.identity.0.principal_id
}

output "web_app_tenant_id" {
  value = azurerm_windows_web_app.webapp_service.identity.0.tenant_id
}

output "default_site_hostname" {
  value = azurerm_windows_web_app.webapp_service.default_hostname
}

output "webapp_name" {
   value =  azurerm_windows_web_app.webapp_service.name
}

output "mock_web_app_object_id" {
  value = var.env_name == "dev" ? azurerm_windows_web_app.mock_webapp_service.0.identity.0.principal_id : null
}

output "default_site_hostname" {
  value = azurerm_windows_web_app.webapp_service.default_hostname
}