output "webapp_name" {
   value = module.webapp_service.webapp_name
}

output "mock_webapp_name" {
   value = local.mock_web_app_name
}