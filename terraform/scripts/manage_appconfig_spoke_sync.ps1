<#
.SYNOPSIS
Manage App Configuration keys and Key Vault secrets for SPOKE_SYNC scenarios.

.DESCRIPTION
This script uses the Azure CLI (`az`) to perform common operations required by the
SPOKE_SYNC process:
    - Create or update an App Configuration key with label `SPOKE_SYNC`.
    - Create or update an Azure Key Vault secret and write its secret-id into App Configuration.
    - Update existing secrets or App Configuration keys.
    - Delete App Configuration keys (label-scoped) and associated Key Vault secrets.

.PARAMETER Operation
The operation to perform. Valid values: `create-key`, `create-secret-link`, `update-secret`, `update-key`, `delete-key-value`, `delete-key-and-secret`.

.PARAMETER AppConfigName
Name of the Azure App Configuration resource to target.

.PARAMETER KeyName
The key name to use in App Configuration.

.PARAMETER KeyValue
The value for the App Configuration key.

.PARAMETER KeyVaultName
The name of the Azure Key Vault.

.PARAMETER SecretName
The name of the secret in Key Vault.

.PARAMETER SecretValue
The value of the Key Vault secret.

.EXAMPLE
# Create an App Configuration key (label SPOKE_SYNC)
.\manage_appconfig_spoke_sync.ps1 -Operation create-key -AppConfigName "myAppConfig" -KeyName "my-key" -KeyValue "some-value"

.EXAMPLE
# Create a Key Vault secret and add its secret id to App Configuration (label SPOKE_SYNC)
.\manage_appconfig_spoke_sync.ps1 -Operation create-secret-link -AppConfigName "myAppConfig" -KeyVaultName "myVault" -SecretName "my-secret" -SecretValue "s3cr3t" -KeyName "kv-secret-key"

.EXAMPLE
# Update the Key Vault secret
.\manage_appconfig_spoke_sync.ps1 -Operation update-secret -KeyVaultName "myVault" -SecretName "my-secret" -SecretValue "new-value"

.EXAMPLE
# Update an App Configuration key's value (non-secret-id one)
.\manage_appconfig_spoke_sync.ps1 -Operation update-key -AppConfigName "myAppConfig" -KeyName "my-key" -KeyValue "new-value"

.EXAMPLE
# Delete specific App Configuration key label
.\manage_appconfig_spoke_sync.ps1 -Operation delete-key-value -AppConfigName "myAppConfig" -KeyName "my-key"

.EXAMPLE
# Delete App Configuration key (label SPOKE_SYNC) and associated Key Vault secret
.\manage_appconfig_spoke_sync.ps1 -Operation delete-key-and-secret -AppConfigName "myAppConfig" -KeyName "kv-secret-key" -KeyVaultName "myVault" -SecretName "my-secret"

.NOTES
- Requires Azure CLI (`az`) and a logged-in session (`az login`).
- The caller must have appropriate RBAC/Access Policy permissions for App Configuration and Key Vault.
- App Configuration operations use the `SPOKE_SYNC` label.
#>

param(
    [Parameter(Mandatory=$true)] [ValidateSet('create-key','create-secret-link','update-secret','update-key','delete-key-value','delete-key-and-secret')] [string]$Operation,
    [string]$AppConfigName,
    [string]$KeyName,
    [string]$KeyValue,
    [string]$KeyVaultName,
    [string]$SecretName,
    [string]$SecretValue
)

function Ensure-AzCli {
    try {
        az --version > $null 2>&1
    } catch {
        Write-Error "Azure CLI (az) is not available. Install and login with 'az login' before running this script."
        exit 2
    }
}

function Set-AppConfigKey {
    param($appConfigName, $key, $value, $label = 'SPOKE_SYNC')
    Write-Host "Setting App Configuration key '$key' (label: $label) in '$appConfigName'..."
    az appconfig kv set --name $appConfigName --key $key --value $value --label $label --only-show-errors | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Failed to set App Configuration key." }
    Write-Host "Done."
}

function Delete-AppConfigKeyLabel {
    param($appConfigName, $key, $label = 'SPOKE_SYNC')
    Write-Host "Deleting App Configuration key '$key' with label '$label' from '$appConfigName'..."
    az appconfig kv delete --name $appConfigName --key $key --label $label --yes --only-show-errors | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Failed to delete App Configuration key label." }
    Write-Host "Done."
}

function Create-KeyVaultSecretAndReturnId {
    param($vaultName, $secretName, $secretValue)
    Write-Host "Creating/Updating Key Vault secret '$secretName' in vault '$vaultName'..."
    $out = az keyvault secret set --vault-name $vaultName --name $secretName --value $secretValue --output json --only-show-errors
    if ($LASTEXITCODE -ne 0) { throw "Failed to create key vault secret." }
    $json = $out | ConvertFrom-Json
    $id = $json.id
    Write-Host "Secret created. id=$id"
    return $id
}

function Update-KeyVaultSecret {
    param($vaultName, $secretName, $secretValue)
    Write-Host "Updating Key Vault secret '$secretName' in vault '$vaultName'..."
    $out = az keyvault secret set --vault-name $vaultName --name $secretName --value $secretValue --output json --only-show-errors
    if ($LASTEXITCODE -ne 0) { throw "Failed to update key vault secret." }
    $json = $out | ConvertFrom-Json
    Write-Host "Secret updated. id=$($json.id)"
}

function Delete-KeyVaultSecret {
    param($vaultName, $secretName)
    Write-Host "Deleting Key Vault secret '$secretName' from vault '$vaultName'..."
    az keyvault secret delete --vault-name $vaultName --name $secretName --only-show-errors | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Failed to delete key vault secret." }
    Write-Host "Secret deleted (soft-delete depends on vault settings)."
}

# Entry
Ensure-AzCli

try {
    switch ($Operation) {
        'create-key' {
            if (-not $AppConfigName -or -not $KeyName -or -not $KeyValue) { throw "Missing parameters for create-key: AppConfigName, KeyName, KeyValue are required." }
            Set-AppConfigKey -appConfigName $AppConfigName -key $KeyName -value $KeyValue
            break
        }
        'create-secret-link' {
            if (-not $AppConfigName -or -not $KeyVaultName -or -not $SecretName -or -not $SecretValue -or -not $KeyName) {
                throw "Missing parameters for create-secret-link: AppConfigName, KeyVaultName, SecretName, SecretValue, KeyName are required." }
            $secretId = Create-KeyVaultSecretAndReturnId -vaultName $KeyVaultName -secretName $SecretName -secretValue $SecretValue
            # Store the secret id in App Configuration under $KeyName with label SPOKE_SYNC
            Set-AppConfigKey -appConfigName $AppConfigName -key $KeyName -value $secretId
            break
        }
        'update-secret' {
            if (-not $KeyVaultName -or -not $SecretName -or -not $SecretValue) { throw "Missing parameters for update-secret: KeyVaultName, SecretName, SecretValue required." }
            Update-KeyVaultSecret -vaultName $KeyVaultName -secretName $SecretName -secretValue $SecretValue
            break
        }
        'update-key' {
            if (-not $AppConfigName -or -not $KeyName -or -not $KeyValue) { throw "Missing parameters for update-key: AppConfigName, KeyName, KeyValue are required." }
            Set-AppConfigKey -appConfigName $AppConfigName -key $KeyName -value $KeyValue
            break
        }
        'delete-key-value' {
            if (-not $AppConfigName -or -not $KeyName) { throw "Missing parameters for delete-key-value: AppConfigName and KeyName are required." }
            Delete-AppConfigKeyLabel -appConfigName $AppConfigName -key $KeyName
            break
        }
        'delete-key-and-secret' {
            if (-not $AppConfigName -or -not $KeyName -or -not $KeyVaultName -or -not $SecretName) { throw "Missing parameters for delete-key-and-secret: AppConfigName, KeyName, KeyVaultName, SecretName are required." }
            # delete the App Config entry with label SPOKE_SYNC
            Delete-AppConfigKeyLabel -appConfigName $AppConfigName -key $KeyName
            # delete the Key Vault secret
            Delete-KeyVaultSecret -vaultName $KeyVaultName -secretName $SecretName
            break
        }
    }
} catch {
    Write-Error "Operation failed: $_"
    exit 1
}

Write-Host "Operation '$Operation' completed successfully."
