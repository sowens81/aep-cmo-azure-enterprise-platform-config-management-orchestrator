Module: Spoke Azure App Configuration

This module provisions a spoke Azure App Configuration instance suitable for use with Managed Identity (RBAC) access patterns.

Design notes:
- Creates only the spoke App Configuration resource.
- Does NOT create keys, service principals, or role assignments — caller should assign `App Configuration Data Owner` to the Function App's Managed Identity.
- Exposes `endpoint` and `id` so callers can configure `APP_CONFIG_ENDPOINT` and perform idempotent RBAC assignments.

Inputs:
- `name`, `resource_group_name`, `location`, `sku`, `tags`

Outputs:
- `id`, `endpoint`, `name`, `resource_group_name`, `location`
