. "$PSScriptRoot/common.ps1"
Require-EnvFile
$values = @{}
Get-Content $EnvFile | ForEach-Object { if ($_ -match '^\s*([^#][^=]*)=(.*)$') { $values[$matches[1].Trim()] = $matches[2].Trim() } }
$apiPort = if ($values.DOTNET_API_PORT) { $values.DOTNET_API_PORT } else { '5000' }
$javaPort = if ($values.JAVA_API_PORT) { $values.JAVA_API_PORT } else { '8080' }
$uiPort = if ($values.REACT_PORT) { $values.REACT_PORT } else { '3000' }
Invoke-Step Platform 'C# Liveness' "GET http://localhost:$apiPort/health/live" { Invoke-WebRequest -UseBasicParsing "http://localhost:$apiPort/health/live" | Out-Null }
Invoke-Step Platform 'C# Readiness' "GET http://localhost:$apiPort/health/ready" { Invoke-WebRequest -UseBasicParsing "http://localhost:$apiPort/health/ready" | Out-Null }
Invoke-Step Platform 'Java Readiness' "GET http://localhost:$javaPort/actuator/health" { Invoke-WebRequest -UseBasicParsing "http://localhost:$javaPort/actuator/health" | Out-Null }
Invoke-Step Platform 'React Health' "GET http://localhost:$uiPort/health" { Invoke-WebRequest -UseBasicParsing "http://localhost:$uiPort/health" | Out-Null }
Set-Location $Root
Invoke-Step Platform 'Container Health' 'docker compose ps' {
  $deadline = (Get-Date).AddSeconds(60)
  do {
    $unhealthy = docker compose -f $Compose --env-file $EnvFile ps --format json | ConvertFrom-Json | Where-Object { $_.Health -ne 'healthy' }
    if (-not $unhealthy) { break }
    Start-Sleep -Seconds 2
  } while ((Get-Date) -lt $deadline)
  if ($unhealthy) { throw "Unhealthy containers: $($unhealthy.Service -join ', ')" }
}
