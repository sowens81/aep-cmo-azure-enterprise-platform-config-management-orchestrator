<#
.SYNOPSIS
    Deploys an Azure Resource Group and a Storage Account for Terraform state, and creates a `tfstate` container using the Azure CLI (`az`).

.DESCRIPTION
    This script will create (or reuse) a resource group and a storage account with a random
    name starting with `stg` (24 characters total), then create a blob container named
    `tfstate` for Terraform remote state. It can also delete the entire resource group and all
    resources when run with the `-Delete` switch.

.PARAMETER SubscriptionId
    The target Azure subscription id where resources will be created or deleted.

.PARAMETER ResourceGroupName
    The name of the Resource Group to create (or delete).

.PARAMETER Region
    The Azure region (location) for the Resource Group and Storage Account (e.g. uksouth).

.PARAMETER Delete
    When present, the script will delete the resource group and all resources within it.

.EXAMPLE
    .\deploy-state-storage.ps1 -SubscriptionId xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx -ResourceGroupName rg-tfstate-dev -Region northeurope

.EXAMPLE
    .\deploy-state-storage.ps1 -SubscriptionId xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx -ResourceGroupName rg-tfstate-dev -Delete

.NOTES
    - Requires the Azure CLI (`az`) installed and authenticated (`az login` or OIDC).
    - Storage account names are lowercase alphanumeric and limited to 24 characters.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$SubscriptionId,

    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,

    [Parameter(Mandatory=$true)]
    [string]$Region,

    [switch]$Delete
)

function New-RandomStorageAccountName {
    param(
        [string]$Prefix = 'stg',
        [int]$TotalLength = 24
    )

    $allowed = 'abcdefghijklmnopqrstuvwxyz0123456789'
    $randomLength = $TotalLength - $Prefix.Length
    if ($randomLength -lt 3) { throw "Prefix too long for desired total length" }

    $chars = 1..$randomLength | ForEach-Object { $allowed[(Get-Random -Maximum $allowed.Length)] }
    return ($Prefix + ($chars -join ''))
}

function Ensure-AzCli {
    if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
        Write-Error "Azure CLI (az) not found in PATH. Install from https://aka.ms/azure-cli and authenticate with 'az login'."
        exit 1
    }
}

Ensure-AzCli

# Ensure subscription context
try {
    az account set --subscription $SubscriptionId 2>$null
} catch {
    Write-Error "Failed to set Azure CLI subscription context to $SubscriptionId. Ensure you are logged in and the subscription id is correct."
    exit 1
}

if ($Delete) {
    # Check RG exists
    $rg = az group show --name $ResourceGroupName --subscription $SubscriptionId --only-show-errors --output json 2>$null | ConvertFrom-Json -ErrorAction SilentlyContinue
    if (-not $rg) {
        Write-Host "Resource group '$ResourceGroupName' not found in subscription $SubscriptionId. Nothing to delete."
        exit 0
    }

    Write-Host "Deleting resource group '$ResourceGroupName' and all contained resources..." -ForegroundColor Yellow
    try {
        az group delete --name $ResourceGroupName --subscription $SubscriptionId --yes --no-wait 2>$null
        Write-Host "Delete started for resource group '$ResourceGroupName' (running asynchronously)." -ForegroundColor Green
        exit 0
    } catch {
        Write-Error "Failed to delete resource group: $_"
        exit 1
    }
}

# Create or ensure resource group
try {
    $rg = az group show --name $ResourceGroupName --subscription $SubscriptionId --output json 2>$null | ConvertFrom-Json -ErrorAction SilentlyContinue
    if (-not $rg) {
        Write-Host "Creating resource group '$ResourceGroupName' in '$Region'..."
        $rgJson = az group create --name $ResourceGroupName --location $Region --subscription $SubscriptionId --output json
        $rg = $rgJson | ConvertFrom-Json
        Write-Host "Resource group created: $($rg.name)" -ForegroundColor Green
    } else {
        Write-Host "Resource group '$ResourceGroupName' already exists. Reusing." -ForegroundColor Cyan
    }
} catch {
    Write-Error "Failed to create/verify resource group: $_"
    exit 1
}

# Create or reuse storage account
$storageAccount = $null
$attempt = 0
$maxAttempts = 8
while (-not $storageAccount -and $attempt -lt $maxAttempts) {
    $attempt++
    $saName = New-RandomStorageAccountName -Prefix 'stg' -TotalLength 24
    Write-Host "Attempt $($attempt): trying storage account name '$saName'..."

    try {
        $existing = az storage account show --name $saName --resource-group $ResourceGroupName --subscription $SubscriptionId --output json 2>$null | ConvertFrom-Json -ErrorAction SilentlyContinue
        if ($existing) { continue }

        $saJson = az storage account create --name $saName --resource-group $ResourceGroupName --location $Region --sku Standard_LRS --kind StorageV2 --subscription $SubscriptionId --output json
        $storageAccount = $saJson | ConvertFrom-Json
        Write-Host "Storage account created: $saName" -ForegroundColor Green
    } catch {
        Write-Warning "Could not create storage account '$saName' (will retry): $($_.Exception.Message)"
        Start-Sleep -Seconds 1
    }
}

if (-not $storageAccount) {
    Write-Warning "Unable to create a new unique storage account after $maxAttempts attempts. Trying to locate existing storage account in the resource group..."
    $listJson = az storage account list --resource-group $ResourceGroupName --subscription $SubscriptionId --output json
    $list = $listJson | ConvertFrom-Json
    $found = $list | Where-Object { $_.name -like 'stg*' } | Select-Object -First 1
    if (-not $found) { Write-Error "No storage account available or created."; exit 1 }
    $saName = $found.name
    $storageAccount = $found
    Write-Host "Reusing existing storage account: $saName" -ForegroundColor Cyan
}

# Ensure blob container 'tfstate' exists
try {
    $keysJson = az storage account keys list --account-name $saName --resource-group $ResourceGroupName --subscription $SubscriptionId --output json
    $keys = $keysJson | ConvertFrom-Json
    $key = $keys[0].value

    $containerCheck = az storage container show --name tfstate --account-name $saName --account-key $key --output json 2>$null | ConvertFrom-Json -ErrorAction SilentlyContinue
    if (-not $containerCheck) {
        Write-Host "Creating blob container 'tfstate'..."
        az storage container create --name tfstate --account-name $saName --account-key $key --output json | Out-Null
        Write-Host "Container 'tfstate' created." -ForegroundColor Green
    } else {
        Write-Host "Container 'tfstate' already exists. Reusing." -ForegroundColor Cyan
    }
} catch {
    Write-Error "Failed to create or verify blob container: $_"
    exit 1
}

# Prepare outputs
$rgShow = az group show --name $ResourceGroupName --subscription $SubscriptionId --output json | ConvertFrom-Json
$rgId = $rgShow.id

$saShow = az storage account show --name $saName --resource-group $ResourceGroupName --subscription $SubscriptionId --output json | ConvertFrom-Json
$saId = $saShow.id

$output = [PSCustomObject]@{
    RESOURCE_GROUP_NAME    = $ResourceGroupName
    RESOURCE_GROUP_ID      = $rgId
    STORAGE_ACCCOUNT_NAME  = $saName
    STORAGE_ACCCOUNTID     = $saId
}

Write-Output ($output | ConvertTo-Json -Depth 5)

exit 0
