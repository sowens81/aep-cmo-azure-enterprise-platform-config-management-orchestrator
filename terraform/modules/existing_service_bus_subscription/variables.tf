variable "topic_name" {
  type        = string
  description = "The name of the Service Bus Topic to create this Subscription in."
}

variable "subscription_name" {
  type        = string
  description = "The name of the Service Bus Subscription to create."
}

variable "servicebus_namespace_name" {
  type        = string
  description = "The name of the Service Bus Namespace where the Topic is located."
}

variable "servicebus_namespace_resource_group_name" {
  type        = string
  description = "The name of the resource group where the Service Bus Topic is located."
}

variable "lock_duration" {
  type        = string
  description = "The lock duration for the subscription as an ISO 8601 duration (e.g. PT30S, PT1M)."
  default     = "PT30S"
}

variable "max_delivery_count" {
  type        = number
  description = "Maximum delivery count for the subscription (recommended 5-10)."
  default     = 5
}

variable "enable_dead_lettering" {
  type        = bool
  description = "Whether dead-lettering is enabled on the subscription."
  default     = true
}

variable "default_message_ttl" {
  type        = string
  description = "Default message time to live as an ISO 8601 duration. Null means service default."
  default     = null
}
