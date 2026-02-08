namespace ConfigManagement.Shared.KeyVault.Authentication;

public enum KeyVaultAuthType
{
    Default,
    ManagedIdentity,
    ClientSecret,
    AzureCli,
    VisualStudio
}
