Module `key_vault`

Creates an Azure Key Vault with RBAC authorization enabled.

Inputs:
- `name`, `resource_group_name`, `location`, `tenant_id` (optional), `tags`

Outputs:
- `id`, `vault_uri`, `name`, `resource_group_name`, `location`

Designed for spoke-only deployments and least-privilege assignments.
