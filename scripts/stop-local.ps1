. "$PSScriptRoot/common.ps1"; Require-EnvFile; Set-Location $Root; docker compose -f $Compose --env-file $EnvFile down
