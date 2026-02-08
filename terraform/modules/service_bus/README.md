# Service Bus module

This module creates an Azure Service Bus namespace and one or more topics. It can optionally create a subscription per-topic to apply dead-lettering (DLQ) related settings such as `max_delivery_count` and `lock_duration`.

Usage example:

```hcl
module "service_bus" {
  source              = "../modules/service_bus"
  namespace_name      = "my-sb-namespace"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  topics = {
    "orders" = {
      create_dlq_subscription = true
      max_delivery_count       = 7
      lock_duration            = "PT30S"
      enable_dead_lettering    = true
    }
    "notifications" = {}
  }
}
```

Notes:
- Azure Service Bus DLQ / delivery settings apply at the subscription level. This module creates a subscription named `<topic>-dlq-sub` when `create_dlq_subscription` is `true` for a topic.
- `max_delivery_count` is recommended to be in the 5–10 range. `lock_duration` should be an ISO8601 duration (`PT30S` or `PT1M`).
