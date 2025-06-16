variable "name" {
  type = string
}

variable "service_name"{
   type = string

 }

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "app_settings" {
  type = map(string)
}

variable "tags" {

}

variable "sku_name" {

}

variable "env_name" {
  type = string
}

variable "mock_webapp_name" {
  type = string
}

variable "addsmock_webapp_name" {
  type = string
}

variable "mock_app_settings" {
  type = map(string)
}

variable "addsmock_app_settings" {
  type = map(string)
}

variable "subnet_id" {
  type = string  
}

variable "deploy_adds_mocks" {
  type = bool  
}
