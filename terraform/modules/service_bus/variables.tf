variable "namespace_name" {
  type        = string
  description = "Name of the Service Bus namespace to create"
}

variable "resource_group_name" {
  type        = string
  description = "Resource group name where the namespace will be created"
}

variable "location" {
  type        = string
  description = "Azure location for the namespace"
}

variable "sku" {
  type        = string
  description = "SKU for the Service Bus namespace (Basic | Standard | Premium)"
  default     = "Standard"
}

variable "capacity" {
  type        = number
  description = "Capacity for the namespace (used with Premium)"
  default     = 1
}

variable "tags" {
  type    = map(string)
  default = {}
}

variable "topics" {
  description = <<-EOT
    Map of topic-name => settings object.
    Example:
      topics = {
        "orders" = {
          enable_partitioning          = false
          enable_express               = false
          default_message_time_to_live = null
        }
      }
  EOT

  type = map(object({
    enable_partitioning          = optional(bool, false)
    enable_express               = optional(bool, false)
    default_message_time_to_live = optional(string, null)
  }))

  default = {}
}
