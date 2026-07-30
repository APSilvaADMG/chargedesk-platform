# Autor: Anderson Pereira Silva
# Data: 29/07/2026
# Descrição: Persistência SQLite da Platform (Azure Files em /data) — share separado do ChargeDesk.
#
# Uso:
#   .\scripts\configurar-persistencia-azure.ps1

[CmdletBinding()]
param(
    [string]$ResourceGroup = "rg-chargedesk",
    [string]$Location = "brazilsouth",
    [string]$EnvironmentName = "cae-chargedesk",
    [string]$AppName = "chargedesk-platform",
    [string]$StorageAccountName = "stchargedesk01",
    [string]$FileShareName = "cdplatform-data",
    [string]$StorageLinkName = "cdplatformdata",
    [string]$VolumeName = "cdplatform-data-vol",
    [string]$MountPath = "/data"
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    throw "Azure CLI nao encontrado."
}

Write-Host ""
Write-Host "=== ChargeDesk Platform — persistencia Azure Files ===" -ForegroundColor Green

$state = az provider show --namespace Microsoft.Storage --query registrationState -o tsv 2>$null
if ($state -ne "Registered") {
    az provider register --namespace Microsoft.Storage --wait | Out-Null
}

$saExists = az storage account show --name $StorageAccountName --resource-group $ResourceGroup 2>$null
if (-not $saExists) {
    az storage account create `
        --name $StorageAccountName `
        --resource-group $ResourceGroup `
        --location $Location `
        --sku Standard_LRS `
        --kind StorageV2 `
        --allow-blob-public-access false | Out-Null
}

$key = az storage account keys list `
    --resource-group $ResourceGroup `
    --account-name $StorageAccountName `
    --query "[0].value" -o tsv

az storage share create `
    --name $FileShareName `
    --account-name $StorageAccountName `
    --account-key $key 2>$null | Out-Null

az containerapp env storage set `
    --name $EnvironmentName `
    --resource-group $ResourceGroup `
    --storage-name $StorageLinkName `
    --azure-file-account-name $StorageAccountName `
    --azure-file-account-key $key `
    --azure-file-share-name $FileShareName `
    --access-mode ReadWrite | Out-Null

$image = az containerapp show `
    --name $AppName `
    --resource-group $ResourceGroup `
    --query "properties.template.containers[0].image" -o tsv

$yaml = @"
properties:
  template:
    volumes:
    - name: $VolumeName
      storageName: $StorageLinkName
      storageType: AzureFile
    containers:
    - name: $AppName
      image: $image
      env:
      - name: ASPNETCORE_ENVIRONMENT
        value: Production
      - name: ASPNETCORE_URLS
        value: http://+:8080
      - name: DB_PATH
        value: /data/platform.db
      - name: TZ
        value: America/Sao_Paulo
      resources:
        cpu: 0.5
        memory: 1Gi
      volumeMounts:
      - volumeName: $VolumeName
        mountPath: $MountPath
    scale:
      minReplicas: 1
      maxReplicas: 1
"@

$yamlFile = Join-Path $env:TEMP "cdplatform-persistencia.yaml"
Set-Content -Path $yamlFile -Value $yaml -Encoding UTF8
az containerapp update --name $AppName --resource-group $ResourceGroup --yaml $yamlFile | Out-Null

$check = az containerapp show `
    --name $AppName `
    --resource-group $ResourceGroup `
    --query "properties.template.containers[0].volumeMounts[0].mountPath" -o tsv

if ($check -ne $MountPath) {
    throw "Volume nao montado em $MountPath (obtido: $check)"
}

Write-Host "Persistencia OK: $StorageAccountName / $FileShareName -> $MountPath" -ForegroundColor Green
