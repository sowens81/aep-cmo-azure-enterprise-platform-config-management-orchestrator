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
  default     = "python"
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
  description = "SKU for the App Service Plan (D1, F1, I1, I2, I3, I1v2, I1mv2, I2v2, I2mv2, I3v2, I3mv2, I4v2, I4mv2, I5v2, I5mv2, I6v2, P1v2, P2v2, P3v2, P0v3, P1v3, P2v3, P3v3, P1mv3, P2mv3, P3mv3, P4mv3, P5mv3, P0v4, P1v4, P2v4, P3v4, P1mv4, P2mv4, P3mv4, P4mv4, P5mv4, S1, S2, S3, SHARED, EP1, EP2, EP3, FC1, WS1, WS2, WS3, and Y1.)"
  type        = string
  default     = "Y1"
}
