# Service Bus Subscription module

Creates a single Azure Service Bus subscription attached to an existing topic.

Inputs
- `topic_id` (string) - topic resource id to attach the subscription to
- `name` (string) - subscription name
- `lock_duration` (string) - ISO8601 duration, default `PT30S`
- `max_delivery_count` (number) - default `5`, clamped to 5..10 by the module
- `enable_dead_lettering` (bool) - default `true`
- `default_message_ttl` (string|null) - optional ISO8601 TTL

Usage example

```hcl
module "config_sync_subscription_spoke_a" {
  source             = "../modules/service_bus_subscription"
  topic_id           = module.service_bus.topics["config-sync"]
  name               = "config-sync-spoke-a"
  lock_duration      = "PT45S"
  max_delivery_count = 7
}
```

This module intentionally manages a single subscription instance so it can be
instantiated multiple times (one per spoke) by callers.
