# Autor: Anderson Pereira Silva
# Data: 30/07/2026
# Descrição: Importa SQLite do ChargeDesk legado para a Platform (admin JWT).

param(
    [string]$CaminhoDb = "C:\Projetos\carregamento-eletrico\carregamento.db",
    [string]$BaseUrl = "http://127.0.0.1:5038",
    [string]$Login = "admin",
    [string]$Senha = "admin123"
)

$ErrorActionPreference = "Stop"
if (-not (Test-Path $CaminhoDb)) { throw "Arquivo não encontrado: $CaminhoDb" }

$loginRes = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/auth/login" `
    -ContentType "application/json" `
    -Body (@{ login = $Login; senha = $Senha } | ConvertTo-Json)

if (-not $loginRes.token) { throw "Login sem token." }
if (-not $loginRes.admin) { throw "Usuário precisa ser Admin para importar." }

$headers = @{ Authorization = "Bearer $($loginRes.token)" }
$body = @{ caminhoDb = (Resolve-Path $CaminhoDb).Path } | ConvertTo-Json

Write-Host "Importando $CaminhoDb ..."
$result = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/admin/importacao/sqlite" `
    -Headers $headers -ContentType "application/json" -Body $body

$result | ConvertTo-Json -Depth 5
Write-Host "Concluído."
