# Compliance Guardian Portal - Local Dev Startup Script (Windows PowerShell)
# Usage: .\portal\start.ps1

$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendDir = Join-Path $RootDir "backend\src\Portal.Api"
$FrontendDir = Join-Path $RootDir "frontend"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " Compliance Guardian Portal - Local Dev" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Check prerequisites
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "ERROR: .NET 8 SDK not found. Install from https://dotnet.microsoft.com/download"
    exit 1
}
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Error "ERROR: Node.js not found. Install from https://nodejs.org"
    exit 1
}

# Install frontend deps if needed
$nodeModules = Join-Path $FrontendDir "node_modules"
if (-not (Test-Path $nodeModules)) {
    Write-Host "[Frontend] Installing dependencies..." -ForegroundColor Yellow
    Push-Location $FrontendDir
    npm install
    Pop-Location
}

# Restore backend
Write-Host "[Backend] Restoring NuGet packages..." -ForegroundColor Yellow
$SrcDir = Join-Path $RootDir "backend\src"
Push-Location $SrcDir
dotnet restore Portal.sln
Pop-Location

# Launch backend
Write-Host "[Backend] Starting Portal.Api on http://localhost:5000 ..." -ForegroundColor Green
$backend = Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$BackendDir'; dotnet run --urls http://localhost:5000" -PassThru

# Launch frontend
Write-Host "[Frontend] Starting Vite dev server on http://localhost:3000 ..." -ForegroundColor Green
$frontend = Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$FrontendDir'; npm run dev" -PassThru

Write-Host ""
Write-Host "Portal is running:" -ForegroundColor Cyan
Write-Host "  Frontend : http://localhost:3000"
Write-Host "  Backend  : http://localhost:5000"
Write-Host "  Swagger  : http://localhost:5000/swagger"
Write-Host ""
Write-Host "Close the opened PowerShell windows to stop the services."
