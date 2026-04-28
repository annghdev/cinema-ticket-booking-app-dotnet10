param(
    [switch]$Build,
    [switch]$Logs,
    [switch]$Status
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
    if (-not (Test-Path $envExampleFile)) {
        throw "Cannot find .env or .env.example in '$scriptDir'."
    }

    Copy-Item $envExampleFile $envFile
    Write-Host "Created .env from .env.example. Review values if needed." -ForegroundColor Yellow
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

if ($Status) {
    Invoke-DevCompose @("ps")
    exit 0
}

if ($Logs) {
    Invoke-DevCompose @("logs", "-f", "--tail", "200")
    exit 0
}

$upArgs = @("up", "-d")
if ($Build) {
    $upArgs += "--build"
}

Write-Host "Starting CinemaTicketBooking development stack..." -ForegroundColor Cyan
Invoke-DevCompose $upArgs

Write-Host ""
Write-Host "Development stack is running:" -ForegroundColor Green
Write-Host "  WebApp:      http://localhost:5173"
Write-Host "  WebServer:   http://localhost:8080"
Write-Host "  Grafana:     http://localhost:3000   (admin/admin by default)"
Write-Host "  Prometheus:  http://localhost:9090"
Write-Host "  Loki ready:  http://localhost:3100/ready"
Write-Host "  Tempo API:   http://localhost:3200"
Write-Host "  PostgreSQL:  localhost:5432"
Write-Host "  Redis:       localhost:6379"
Write-Host "  cAdvisor:    http://localhost:8081"
Write-Host ""
Write-Host "Use './down-clean.ps1' for a full cleanup of containers, network, and volumes." -ForegroundColor Yellow
