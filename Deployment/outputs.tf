output "webapp_name" {
   value = module.webapp_service.webapp_name
}

output "mock_webapp_name" {
   value = local.env_name == "dev" ? local.mock_web_app_name : null
}

output "storage_connection_string" {
   value = module.storage.storage_connection_string
   sensitive = true
}


output "resource_group" {
  value = azurerm_resource_group.rg.name
}

output "erp_facade_web_app_url" {
  value = "https://${module.webapp_service.default_site_hostname}"
}