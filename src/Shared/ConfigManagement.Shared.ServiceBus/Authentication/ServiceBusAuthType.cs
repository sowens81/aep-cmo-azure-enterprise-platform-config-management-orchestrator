namespace ConfigManagement.Shared.ServiceBus.Authentication;

public enum ServiceBusAuthType
{
    Default,
    ClientSecret,
    ManagedIdentity,
    VisualStudio,
    AzureCli
}
