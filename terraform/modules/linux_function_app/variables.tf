variable "name" {
  description = "Function App name"
  type        = string
}

variable "resource_group_name" {
  description = "Resource group for all resources"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "identity_name" {
  description = "User-assigned identity name"
  type        = string
}

variable "storage_account_name" {
  description = "Storage account name for the Function App"
  type        = string
}

variable "worker_runtime" {
  description = "Functions worker runtime (eg. python, dotnet)"
  type        = string
  default     = "dotnet"
}

variable "function_version" {
  description = "Functions runtime version"
  type        = string
  default     = "~4"
}

variable "app_settings" {
  description = "Additional app settings map"
  type        = map(string)
  default     = {}
}

variable "website_run_from_package" {
  description = "Whether to set WEBSITE_RUN_FROM_PACKAGE"
  type        = bool
  default     = true
}

variable "tags" {
  description = "Tags for resources"
  type        = map(string)
  default     = {}
}

variable "app_service_plan_sku" {
  description = "SKU for the App Service Plan (tbd)"
  type        = string
  default     = "FC1"
}
