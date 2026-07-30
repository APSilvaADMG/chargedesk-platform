# Autor: Anderson Pereira Silva
# Data: 29/07/2026
# Descrição: Build da imagem no ACR e deploy no Azure Container Apps (Platform).
#
# Uso:
#   .\scripts\deploy-azure.ps1 -CriarRecursos
#   .\scripts\deploy-azure.ps1
#
# App separado do ChargeDesk atual (chargedesk) para não sobrescrever produção.

[CmdletBinding()]
param(
    [string]$SubscriptionId = "",
    [string]$ResourceGroup = "rg-chargedesk",
    [string]$Location = "brazilsouth",
    [string]$AppName = "chargedesk-platform",
    [string]$EnvironmentName = "cae-chargedesk",
    [string]$AcrName = "acrchargedesk01",
    [string]$ImageName = "chargedesk-platform",
    [string]$ImageTag = "latest",
    [double]$Cpu = 0.5,
    [string]$Memory = "1.0Gi",
    [int]$MinReplicas = 1,
    [int]$MaxReplicas = 1,
    [int]$TargetPort = 8080,
    [switch]$SomenteBuild,
    [switch]$CriarRecursos
)

$ErrorActionPreference = "Stop"

function Require-AzCli {
    if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
        throw "Azure CLI não encontrado. Instale: https://learn.microsoft.com/cli/azure/install-azure-cli"
    }
}

function Get-RepoRoot {
    $root = Resolve-Path (Join-Path $PSScriptRoot "..")
    return $root.Path
}

Require-AzCli

if ($SubscriptionId) {
    az account set --subscription $SubscriptionId | Out-Null
}

$account = az account show -o json | ConvertFrom-Json
Write-Host "Assinatura: $($account.name) ($($account.id))" -ForegroundColor Cyan

$repoRoot = Get-RepoRoot
$imageFull = "${ImageName}:${ImageTag}"
$acrLoginServer = "${AcrName}.azurecr.io"
$imageUri = "$acrLoginServer/$imageFull"

Write-Host ""
Write-Host "=== ChargeDesk Platform - deploy Azure Container Apps ===" -ForegroundColor Green
Write-Host "Grupo: $ResourceGroup | Regiao: $Location | App: $AppName"
Write-Host "ACR: $AcrName | Imagem: $imageUri"
Write-Host ""

if (-not (az group exists --name $ResourceGroup)) {
    if (-not $CriarRecursos) {
        throw "Grupo '$ResourceGroup' nao existe. Rode com -CriarRecursos."
    }
    Write-Host "Criando grupo de recursos..." -ForegroundColor Yellow
    az group create --name $ResourceGroup --location $Location | Out-Null
}

$acrExists = az acr show --name $AcrName --resource-group $ResourceGroup 2>$null
if (-not $acrExists) {
    if (-not $CriarRecursos) {
        throw "ACR '$AcrName' nao encontrado. Rode com -CriarRecursos."
    }
    Write-Host "Criando Azure Container Registry..." -ForegroundColor Yellow
    az acr create `
        --resource-group $ResourceGroup `
        --name $AcrName `
        --sku Basic `
        --location $Location `
        --admin-enabled true | Out-Null
}

Write-Host "Build e push da imagem no ACR..." -ForegroundColor Yellow
if ($ImageTag -eq "latest") {
    $ImageTag = (Get-Date).ToString("yyyyMMddHHmmss")
    $imageFull = "${ImageName}:${ImageTag}"
    $imageUri = "$acrLoginServer/$imageFull"
    Write-Host "Tag de deploy: $ImageTag" -ForegroundColor Cyan
}

Push-Location $repoRoot
try {
    $built = $false
    if (Get-Command docker -ErrorAction SilentlyContinue) {
        Write-Host "Build local via Docker..." -ForegroundColor Cyan
        az acr login --name $AcrName --resource-group $ResourceGroup | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "az acr login falhou" }

        docker build -t $imageUri -t "${acrLoginServer}/${ImageName}:latest" -f Dockerfile .
        if ($LASTEXITCODE -ne 0) { throw "docker build falhou" }

        docker push $imageUri
        if ($LASTEXITCODE -ne 0) { throw "docker push ($ImageTag) falhou" }

        docker push "${acrLoginServer}/${ImageName}:latest"
        if ($LASTEXITCODE -ne 0) { throw "docker push (latest) falhou" }

        $built = $true
    }

    if (-not $built) {
        Write-Host "Tentando az acr build..." -ForegroundColor Yellow
        az acr build `
            --resource-group $ResourceGroup `
            --registry $AcrName `
            --image $imageFull `
            --file Dockerfile `
            .
        if ($LASTEXITCODE -ne 0) {
            throw "az acr build falhou. Instale Docker Desktop ou habilite ACR Tasks."
        }
    }
}
finally {
    Pop-Location
}

Write-Host "Imagem publicada: $imageUri" -ForegroundColor Green

if ($SomenteBuild) {
    Write-Host "SomenteBuild: deploy ignorado." -ForegroundColor Cyan
    exit 0
}

$creds = az acr credential show --name $AcrName --resource-group $ResourceGroup -o json | ConvertFrom-Json
$registryUser = $creds.username
$registryPass = $creds.passwords[0].value

$appExists = az containerapp show --name $AppName --resource-group $ResourceGroup 2>$null

if (-not $appExists) {
    if (-not $CriarRecursos) {
        Write-Host "Container App '$AppName' nao existe. Rode com -CriarRecursos." -ForegroundColor Yellow
        exit 0
    }

    $envExists = az containerapp env show --name $EnvironmentName --resource-group $ResourceGroup 2>$null
    if (-not $envExists) {
        Write-Host "Criando Container Apps Environment..." -ForegroundColor Yellow
        az containerapp env create `
            --name $EnvironmentName `
            --resource-group $ResourceGroup `
            --location $Location | Out-Null
    }

    Write-Host "Criando Container App..." -ForegroundColor Yellow
    az containerapp create `
        --name $AppName `
        --resource-group $ResourceGroup `
        --environment $EnvironmentName `
        --image $imageUri `
        --registry-server $acrLoginServer `
        --registry-username $registryUser `
        --registry-password $registryPass `
        --target-port $TargetPort `
        --ingress external `
        --cpu $Cpu `
        --memory $Memory `
        --min-replicas $MinReplicas `
        --max-replicas $MaxReplicas `
        --env-vars `
            "ASPNETCORE_ENVIRONMENT=Production" `
            "ASPNETCORE_URLS=http://+:8080" `
            "DB_PATH=/data/platform.db" `
            "TZ=America/Sao_Paulo" | Out-Null
}
else {
    Write-Host "Atualizando Container App existente..." -ForegroundColor Yellow
    $revisionSuffix = (Get-Date).ToString("yyyyMMddHHmmss")
    az containerapp update `
        --name $AppName `
        --resource-group $ResourceGroup `
        --image $imageUri `
        --revision-suffix $revisionSuffix `
        --cpu $Cpu `
        --memory $Memory `
        --min-replicas $MinReplicas `
        --max-replicas $MaxReplicas `
        --set-env-vars "TZ=America/Sao_Paulo" | Out-Null

    az containerapp ingress update `
        --name $AppName `
        --resource-group $ResourceGroup `
        --type external `
        --target-port $TargetPort | Out-Null
}

$fqdn = az containerapp show `
    --name $AppName `
    --resource-group $ResourceGroup `
    --query "properties.configuration.ingress.fqdn" `
    -o tsv

Write-Host ""
Write-Host "Deploy concluido." -ForegroundColor Green
if ($fqdn) {
    Write-Host "URL: https://$fqdn"
    Write-Host "Health: https://$fqdn/api/health"
}
Write-Host ""
$mountPath = az containerapp show `
    --name $AppName `
    --resource-group $ResourceGroup `
    --query "properties.template.containers[0].volumeMounts[?mountPath=='/data'].mountPath | [0]" `
    -o tsv 2>$null
if ($mountPath -eq "/data") {
    Write-Host "Persistencia: volume Azure Files montado em /data." -ForegroundColor Green
} else {
    Write-Host "Persistencia:" -ForegroundColor Yellow
    Write-Host "  Volume /data NAO montado — rode .\scripts\configurar-persistencia-azure.ps1"
}
