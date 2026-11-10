variable "location" {
  type    = string
  default = "uksouth"
}

variable "resource_group_name" {
  type    = string
  default = "erpfacade"
}

variable "subscription_id" {
  type = string
}

variable "hub_subscription_id" {
  type = string
}

variable "dns_zone_rg" {
  type = string  
}

variable "sku_name" {
  type = map(any)
  default = {
            "dev"     =  "P1v2"            
            "vni"     =  "P1v3"
            "iat"     =  "P1v3"
            "prp"     =  "P1v3"     
            "vne"     =  "P1v3"
            live      =  "P1v3"
            }
}

variable "spoke_rg" {
  type = string
}

variable "pe_rg" {
  type = string
}

variable "spoke_vnet_name" {
  type = string
}

variable "spoke_subnet_name" {
  type = string
}

variable "pe_vnet_name" {
  type = string
}

variable "pe_subnet_name" {
  type = string
}

variable "deploy_adds_mocks" {
  type = bool  
}
variable "PermitDecryptionHardwareId" {
  type = string
}

# Front Door; set per environment in config/<workspace>.tfvars.
variable "front_door_enabled" {
  description = "Whether to configure Front Door delivery routing for the ERP Facade web app."
  type        = bool
  default     = false
}

variable "front_door_route_enabled" {
  description = "Whether the Front Door route is enabled. Allows the route to be disabled without removing resources. Only applies when front_door_enabled is true."
  type        = bool
  default     = true
}

variable "front_door_subscription_id" {
  description = "The subscription ID where the shared Azure Front Door profile is deployed."
  type        = string
  default     = ""
}

variable "front_door_profile_name" {
  description = "The name of the existing shared Azure Front Door profile created by the platform team."
  type        = string
  default     = ""
}

variable "front_door_resource_group" {
  description = "The resource group containing the shared Azure Front Door profile."
  type        = string
  default     = ""
}

variable "front_door_shared_endpoint_name" {
  description = "The name of the shared Front Door endpoint to attach routes to."
  type        = string
  default     = ""
}

variable "front_door_custom_domain_hostname" {
  description = "The public hostname for the custom domain. Leave empty to use the default AFD endpoint hostname."
  type        = string
  default     = ""
}

variable "front_door_custom_domain_dns_zone_id" {
  description = "The resource ID of the Azure DNS zone used for custom domain validation. May be empty while DNS is hosted outside Azure."
  type        = string
  default     = ""
}
