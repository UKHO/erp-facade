output "webapp_name" {
   value = module.webapp_service.webapp_name
}

output "mock_webapp_name" {
   value = local.env_name == "dev" ? local.mock_web_app_name : null
}

output "resource_group" {
  value = azurerm_resource_group.rg.name
}