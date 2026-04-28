param(
    [switch]$RemoveBuiltImages
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$composeFile = Join-Path $scriptDir "docker-compose.yml"
$envFile = Join-Path $scriptDir ".env"
$envExampleFile = Join-Path $scriptDir ".env.example"
$projectName = "cinema-ticket-booking-dev"

if (-not (Test-Path $composeFile)) {
    throw "Cannot find docker-compose.yml at '$composeFile'."
}

if (-not (Test-Path $envFile)) {
    $envFile = $envExampleFile
}

if (-not (Test-Path $envFile)) {
    throw "Cannot find .env or .env.example in '$scriptDir'."
}

function Assert-DockerReady {
    try {
        docker info | Out-Null
    }
    catch {
        throw "Docker daemon is not available. Start Docker Desktop first."
    }
}

function Invoke-DevCompose([string[]]$ComposeArgs) {
    docker compose --project-name $projectName --env-file $envFile -f $composeFile @ComposeArgs
}

Assert-DockerReady

$downArgs = @("down", "--volumes", "--remove-orphans")
if ($RemoveBuiltImages) {
    $downArgs += @("--rmi", "local")
}

Write-Host "Stopping and cleaning CinemaTicketBooking development stack..." -ForegroundColor Yellow
Invoke-DevCompose $downArgs

Write-Host "Cleanup completed. Containers, project network, and volumes were removed." -ForegroundColor Green
if (-not $RemoveBuiltImages) {
    Write-Host "Built images were kept. Re-run with -RemoveBuiltImages to remove local compose-built images too." -ForegroundColor Yellow
}
