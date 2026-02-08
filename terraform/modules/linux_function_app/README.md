Module: Function App (spoke)

This module provisions a Linux Consumption Azure Function App with a User-Assigned Managed Identity and the required supporting resources (Storage Account and App Service Plan). It does NOT create App Configuration or Key Vault; assign RBAC to the managed identity from the caller scope.

Inputs:
- `name`, `resource_group_name`, `location`, `identity_name`
- `worker_runtime`, `linux_fx_version` and optional `app_settings`

Outputs:
- `function_app_id`, `identity_id`, `identity_principal_id`
