variable "name" {
  description = "Key Vault name"
  type        = string
}

variable "resource_group_name" {
  description = "Resource group where Key Vault will be created"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "tenant_id" {
  description = "Tenant id for Key Vault (defaults to caller tenant)"
  type        = string
  default     = ""
}

variable "sku" {
  description = "Key Vault SKU (standard|premium)"
  type        = string
  default     = "standard"
}

variable "purge_protection_enabled" {
  description = "Enable purge protection"
  type        = bool
  default     = false
}

variable "soft_delete_enabled" {
  description = "Enable soft delete"
  type        = bool
  default     = true
}

variable "tags" {
  description = "Tags for the Key Vault"
  type        = map(string)
  default     = {}
}

variable "soft_delete_retention_days" {
  description = "Number of days to retain deleted Key Vault (defaults to 7)"
  type        = number
  default     = 0

}
