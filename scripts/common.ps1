$ErrorActionPreference = 'Stop'
$Script:Root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$Script:Compose = Join-Path $Root 'commerce-platform-infra/compose.yml'
$Script:EnvFile = Join-Path $Root '.env.local'
function Require-EnvFile { if (-not (Test-Path $EnvFile)) { throw "Missing $EnvFile. Copy .env.local.example to .env.local first." } }
function Invoke-Step([string]$Project,[string]$Stage,[string]$Command,[scriptblock]$Action) {
  Write-Host "[$Project] $Stage`n> $Command"
  try {
    $global:LASTEXITCODE = 0
    & $Action
    if ($LASTEXITCODE -ne 0) { throw "ExitCode=$LASTEXITCODE" }
  } catch {
    Write-Error "Project=$Project; Stage=$Stage; Command=$Command; Error=$($_.Exception.Message)"
    throw
  }
}
