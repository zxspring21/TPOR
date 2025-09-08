# Test Environment Setup
# This script tests the environment setup for TPOR Intranet services

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("development-local", "development-docker", "production")]
    [string]$Environment
)

# Colors for output
$ErrorColor = "Red"
$SuccessColor = "Green"
$WarningColor = "Yellow"
$InfoColor = "Blue"

Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host "  TPOR Intranet Environment Test" -ForegroundColor $InfoColor
Write-Host "  Environment: $Environment" -ForegroundColor $InfoColor
Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host ""

# Test 1: Environment Configuration Files
Write-Host "Test 1: Environment Configuration Files" -ForegroundColor $WarningColor
$apiConfigFile = "env-configs/api-$Environment.env.example"
$workerConfigFile = "env-configs/worker-$Environment.env.example"

if (Test-Path $apiConfigFile) {
    Write-Host "✓ API configuration file exists: $apiConfigFile" -ForegroundColor $SuccessColor
} else {
    Write-Host "✗ API configuration file missing: $apiConfigFile" -ForegroundColor $ErrorColor
}

if (Test-Path $workerConfigFile) {
    Write-Host "✓ Worker configuration file exists: $workerConfigFile" -ForegroundColor $SuccessColor
} else {
    Write-Host "✗ Worker configuration file missing: $workerConfigFile" -ForegroundColor $ErrorColor
}

# Test 2: Environment Setup Script
Write-Host ""
Write-Host "Test 2: Environment Setup Script" -ForegroundColor $WarningColor
$setupScript = "scripts/setup-environment.ps1"
if (Test-Path $setupScript) {
    Write-Host "✓ Environment setup script exists: $setupScript" -ForegroundColor $SuccessColor
} else {
    Write-Host "✗ Environment setup script missing: $setupScript" -ForegroundColor $ErrorColor
}

# Test 3: Test Scripts
Write-Host ""
Write-Host "Test 3: Test Scripts" -ForegroundColor $WarningColor
$apiTestScript = "scripts/test-api-by-environment.ps1"
$workerTestScript = "scripts/test-worker-by-environment.ps1"

if (Test-Path $apiTestScript) {
    Write-Host "✓ API test script exists: $apiTestScript" -ForegroundColor $SuccessColor
} else {
    Write-Host "✗ API test script missing: $apiTestScript" -ForegroundColor $ErrorColor
}

if (Test-Path $workerTestScript) {
    Write-Host "✓ Worker test script exists: $workerTestScript" -ForegroundColor $SuccessColor
} else {
    Write-Host "✗ Worker test script missing: $workerTestScript" -ForegroundColor $ErrorColor
}

# Test 4: Project Structure
Write-Host ""
Write-Host "Test 4: Project Structure" -ForegroundColor $WarningColor
$apiProject = "src/TPOR.Intranet.API/TPOR.Intranet.API.csproj"
$workerProject = "src/TPOR.Intranet.Worker/TPOR.Intranet.Worker.csproj"
$sharedProject = "src/TPOR.Shared/TPOR.Shared.csproj"

if (Test-Path $apiProject) {
    Write-Host "✓ API project exists: $apiProject" -ForegroundColor $SuccessColor
} else {
    Write-Host "✗ API project missing: $apiProject" -ForegroundColor $ErrorColor
}

if (Test-Path $workerProject) {
    Write-Host "✓ Worker project exists: $workerProject" -ForegroundColor $SuccessColor
} else {
    Write-Host "✗ Worker project missing: $workerProject" -ForegroundColor $ErrorColor
}

if (Test-Path $sharedProject) {
    Write-Host "✓ Shared project exists: $sharedProject" -ForegroundColor $SuccessColor
} else {
    Write-Host "✗ Shared project missing: $sharedProject" -ForegroundColor $ErrorColor
}

# Test 5: Environment-Specific Requirements
Write-Host ""
Write-Host "Test 5: Environment-Specific Requirements" -ForegroundColor $WarningColor

switch ($Environment) {
    "development-local" {
        Write-Host "Development Local Requirements:" -ForegroundColor $InfoColor
        Write-Host "  - No database dependency for API service" -ForegroundColor $InfoColor
        Write-Host "  - Local file storage" -ForegroundColor $InfoColor
        Write-Host "  - Mock message queue" -ForegroundColor $InfoColor
        Write-Host "  - JWT authentication with local secrets" -ForegroundColor $InfoColor
    }
    
    "development-docker" {
        Write-Host "Development Docker Requirements:" -ForegroundColor $InfoColor
        Write-Host "  - Docker MySQL database" -ForegroundColor $InfoColor
        Write-Host "  - Local file storage" -ForegroundColor $InfoColor
        Write-Host "  - Mock message queue" -ForegroundColor $InfoColor
        Write-Host "  - JWT authentication with local secrets" -ForegroundColor $InfoColor
    }
    
    "production" {
        Write-Host "Production Requirements:" -ForegroundColor $InfoColor
        Write-Host "  - Google Cloud SQL database" -ForegroundColor $InfoColor
        Write-Host "  - Google Cloud Storage" -ForegroundColor $InfoColor
        Write-Host "  - Google Pub/Sub message queue" -ForegroundColor $InfoColor
        Write-Host "  - JWT authentication with Google Secret Manager" -ForegroundColor $InfoColor
    }
}

Write-Host ""
Write-Host "Environment test completed!" -ForegroundColor $SuccessColor
Write-Host ""
Write-Host "Next steps:" -ForegroundColor $WarningColor
Write-Host "  1. Run: pwsh scripts/setup-environment.ps1 -Environment $Environment" -ForegroundColor $WarningColor
Write-Host "  2. Test API: pwsh scripts/test-api-by-environment.ps1 -Environment $Environment" -ForegroundColor $WarningColor
Write-Host "  3. Test Worker: pwsh scripts/test-worker-by-environment.ps1 -Environment $Environment" -ForegroundColor $WarningColor
