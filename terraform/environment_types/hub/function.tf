module "function" {
  count                = var.local_development ? 0 : 1
  source               = "../../modules/linux_function_app"
  name                 = var.function_name
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  identity_name        = var.identity_name
  storage_account_name = var.storage_account_name
  app_settings = {

    # -------------------------------------------------
    # Service Bus trigger bindings (FLAT KEYS REQUIRED)
    # -------------------------------------------------
    ServiceBusConnection         = "Endpoint=sb://${var.servicebus_config.namespace_name}.servicebus.windows.net/"
    SBUS_APP_CONFIG_TOPIC        = var.servicebus_config.app_config_event_topic_name
    SBUS_APP_CONFIG_SUBSCRIPTION = module.servicebus_subscription_app_config_event.subscription_name
    SBUS_KEY_VAULT_TOPIC         = var.servicebus_config.key_vault_event_topic_name
    SBUS_KEY_VAULT_SUBSCRIPTION  = module.servicebus_subscription_key_vault_event.subscription_name

    # -------------------------------------------------
    # Environment Context configuration
    # -------------------------------------------------
    Organisation    = var.organisation
    Region          = var.location
    EnvironmentTier = var.environment_tier
    EnvironmentName = var.environment
    ServiceName     = "config-event-orchestrator"

    # -----------------------------
    # Key Vault configuration
    # -----------------------------
    KeyVault__Endpoint = "${module.key_vault.vault_uri}"

    KeyVault__Auth__AuthType = "ManagedIdentity"

    # -----------------------------
    # App Configuration configuration
    # -----------------------------
    "AppConfiguration__Endpoint"       = "${module.app_configuration.endpoint}"
    "AppConfiguration__Auth__AuthType" = "ManagedIdentity"

    # -----------------------------
    # Service Bus configuration
    # -----------------------------
    "ServiceBus__Endpoint"                         = "${var.servicebus_config.namespace_name}.servicebus.windows.net"
    "ServiceBus__Auth__AuthType"                   = "ManagedIdentity"
    "ServiceBus__Topics__KeyVaultSync__TopicName"  = "key-vault-Sync"
    "ServiceBus__Topics__AppConfigSync__TopicName" = "app-config-Sync"
  }
}