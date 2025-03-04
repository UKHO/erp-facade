output "webapp_name" {
  value =  module.webapp_service.webapp_name
}

output "mock_webapp_name" {
  value = local.env_name == "dev" ? local.mock_web_app_name : null
}

output "addsmock_webapp_name" {
  value = local.addsmock_web_app_name
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

output "erp_facade_web_app_public_url" {
  value = "https://erpfacade${local.env_name}.admiralty.co.uk"
}

output "erp_facade_mock_service_url" {
  value = local.env_name == "dev" ? "https://${module.webapp_service.default_site_hostname_mock}" : null
}

output keyvault_uri {
  value = module.key_vault.keyvault_uri
}

output "webapp_slot_name" {
  value = module.webapp_service.slot_name
}

output "webapp_slot_default_site_hostname" {
  value = module.webapp_service.slot_default_site_hostname
}
