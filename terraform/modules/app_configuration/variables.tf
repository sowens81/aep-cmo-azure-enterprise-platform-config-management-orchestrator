variable "name" {
  description = "App Configuration name"
  type        = string
}

variable "resource_group_name" {
  description = "Resource group where App Configuration will be created"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "sku" {
  description = "SKU for App Configuration (free|developer|standard)"
  type        = string
  default     = "free"
  validation {
    condition     = contains(["free", "developer", "standard"], var.sku)
    error_message = "sku must be either 'free', 'developer', or 'standard'"
  }
}

variable "tags" {
  description = "Tags applied to the resource"
  type        = map(string)
  default     = {}
}
