variable "environment" {
  description = "Deployment environment (e.g. dev, test, prod)"
  type        = string

}

variable "organisation" {
  description = "Name of the organisation (xxxx, xxyy)"
  type        = string
}

variable "tenant_id" {
  description = "Azure Active Directory tenant ID"
  type        = string
}


variable "hub_subscription_id" {
  description = "Subscription ID of the hub where the Service Bus namespace is provisioned"
  type        = string
}

variable "environment_subscription_id" {
  description = "Subscription ID where spoke resources will be provisioned"
  type        = string
}

variable "resource_group_name" {
  description = "Spoke resource group name"
  type        = string
}

variable "location" {
  description = "Location for spoke resources"
  type        = string
  default     = "uksouth"
}

variable "entraid_spoke_access_group_name" {
  description = "Name of the Entra ID group that will be granted access to the Service Bus topics in the hub"
  type        = string
  default     = "so-demo-spoke-config-management-consumers-entraid-group"

}

variable "function_name" {
  description = "Name for the Function App"
  type        = string
}

variable "identity_name" {
  description = "User assigned identity name"
  type        = string
}

variable "storage_account_name" {
  description = "Storage account name for Function App (lowercase)"
  type        = string
}

variable "app_configuration_name" {
  description = "Spoke App Configuration name"
  type        = string
}

variable "key_vault_name" {
  description = "Spoke Key Vault name"
  type        = string
}

variable "app_configuration_sku" {
  description = "SKU for App Configuration (free|standard)"
  type        = string
  default     = "free"
}

variable "tags" {
  description = "Tags applied to spoke resources"
  type        = map(string)
  default     = {}
}

variable "idempotency_table" {
  description = "Name of the idempotency table in the storage account"
  type        = string
  default     = "configSyncIdempotency"
}

variable "servicebus_config" {
  description = "Service Bus configuration for the Function App to publish sync messages. Should include the namespace endpoint and topic/subscription names."
  type = object({
    resource_group_name = string
    namespace_name      = string
    sync_topic_name     = string
    app_config_event_topic_name    = string
    key_vault_event_topic_name = string
    result_topic_name   = string
  })
}

