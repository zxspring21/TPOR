# Environment Setup Script
# This script sets up the environment for TPOR Intranet services

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("development-local", "development-docker", "production")]
    [string]$Environment,
    
    [string]$ConfigPath = "env-configs"
)

# Colors for output
$ErrorColor = "Red"
$SuccessColor = "Green"
$WarningColor = "Yellow"
$InfoColor = "Blue"

Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host "  TPOR Intranet Environment Setup" -ForegroundColor $InfoColor
Write-Host "  Environment: $Environment" -ForegroundColor $InfoColor
Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host ""

# Function to copy environment files
function Copy-EnvironmentFiles {
    param(
        [string]$SourcePath,
        [string]$TargetPath
    )
    
    if (Test-Path $SourcePath) {
        Copy-Item $SourcePath $TargetPath -Force
        Write-Host "✓ Copied environment file to $TargetPath" -ForegroundColor $SuccessColor
        return $true
    } else {
        Write-Host "✗ Environment file not found: $SourcePath" -ForegroundColor $ErrorColor
        return $false
    }
}

# Setup API environment
Write-Host "Setting up API environment..." -ForegroundColor $InfoColor
$apiSourceFile = "$ConfigPath/api-$Environment.env.example"
$apiTargetFile = "src/TPOR.Intranet.API/.env"

if (Copy-EnvironmentFiles $apiSourceFile $apiTargetFile) {
    Write-Host "  API environment configured for $Environment" -ForegroundColor $SuccessColor
} else {
    Write-Host "  Failed to configure API environment" -ForegroundColor $ErrorColor
}

Write-Host ""

# Setup Worker environment
Write-Host "Setting up Worker environment..." -ForegroundColor $InfoColor
$workerSourceFile = "$ConfigPath/worker-$Environment.env.example"
$workerTargetFile = "src/TPOR.Intranet.Worker/.env"

if (Copy-EnvironmentFiles $workerSourceFile $workerTargetFile) {
    Write-Host "  Worker environment configured for $Environment" -ForegroundColor $SuccessColor
} else {
    Write-Host "  Failed to configure Worker environment" -ForegroundColor $ErrorColor
}

Write-Host ""

# Environment-specific setup
switch ($Environment) {
    "development-local" {
        Write-Host "Development Local Setup:" -ForegroundColor $InfoColor
        Write-Host "  - No database dependency for both API and Worker services" -ForegroundColor $InfoColor
        Write-Host "  - Local file storage" -ForegroundColor $InfoColor
        Write-Host "  - Mock message queue" -ForegroundColor $InfoColor
        Write-Host "  - JWT authentication with local secrets" -ForegroundColor $InfoColor
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor $WarningColor
        Write-Host "  1. Start API: cd src/TPOR.Intranet.API && dotnet run" -ForegroundColor $WarningColor
        Write-Host "  2. Start Worker: cd src/TPOR.Intranet.Worker && dotnet run" -ForegroundColor $WarningColor
        Write-Host "  3. Test services: pwsh scripts/test-api-by-environment.ps1 -Environment development-local" -ForegroundColor $WarningColor
    }
    
    "development-docker" {
        Write-Host "Development Docker Setup:" -ForegroundColor $InfoColor
        Write-Host "  - Docker MySQL database" -ForegroundColor $InfoColor
        Write-Host "  - Local file storage" -ForegroundColor $InfoColor
        Write-Host "  - Mock message queue" -ForegroundColor $InfoColor
        Write-Host "  - JWT authentication with local secrets" -ForegroundColor $InfoColor
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor $WarningColor
        Write-Host "  1. Run: docker-compose up -d" -ForegroundColor $WarningColor
        Write-Host "  2. Wait for MySQL to be ready" -ForegroundColor $WarningColor
        Write-Host "  3. Start services: docker-compose up" -ForegroundColor $WarningColor
    }
    
    "production" {
        Write-Host "Production Setup:" -ForegroundColor $InfoColor
        Write-Host "  - Google Cloud SQL database" -ForegroundColor $InfoColor
        Write-Host "  - Google Cloud Storage" -ForegroundColor $InfoColor
        Write-Host "  - Google Pub/Sub message queue" -ForegroundColor $InfoColor
        Write-Host "  - JWT authentication with Google Secret Manager" -ForegroundColor $InfoColor
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor $WarningColor
        Write-Host "  1. Configure Google Cloud credentials" -ForegroundColor $WarningColor
        Write-Host "  2. Set up secrets in Google Secret Manager" -ForegroundColor $WarningColor
        Write-Host "  3. Deploy to Google Cloud Run: pwsh scripts/deploy.sh" -ForegroundColor $WarningColor
    }
}

Write-Host ""
Write-Host "Environment setup completed!" -ForegroundColor $SuccessColor
