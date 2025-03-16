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
  value = azurerm_windows_web_app.webapp_service.name
}

output "slot_object_id" {
  value = azurerm_windows_web_app_slot.staging.identity.0.principal_id
}

output "slot_tenant_id" {
  value = azurerm_windows_web_app_slot.staging.identity.0.tenant_id
}

output "slot_default_site_hostname" {
  value = azurerm_windows_web_app_slot.staging.default_hostname
}

output "slot_name" {
  value = azurerm_windows_web_app_slot.staging.name
}

output "mock_web_app_object_id" {
  value = var.env_name == "dev" || var.env_name == "iat" ? azurerm_windows_web_app.mock_webapp_service.0.identity.0.principal_id : null
}

output "default_site_hostname_mock" {
  value = var.env_name == "dev" || var.env_name == "iat"  ? azurerm_windows_web_app.mock_webapp_service[0].default_hostname : null
}
