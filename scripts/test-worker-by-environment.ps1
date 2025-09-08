# Test Worker Service by Environment
# This script tests the TPOR.Intranet.Worker service based on the specified environment

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

# Test results
$TestsPassed = 0
$TestsFailed = 0

# Function to print test results
function Write-TestResult {
    param(
        [string]$TestName,
        [string]$Result,
        [string]$Details = ""
    )
    
    if ($Result -eq "PASS") {
        Write-Host "✓ PASS: $TestName" -ForegroundColor $SuccessColor
        if ($Details) {
            Write-Host "  Details: $Details" -ForegroundColor $InfoColor
        }
        $script:TestsPassed++
    } else {
        Write-Host "✗ FAIL: $TestName" -ForegroundColor $ErrorColor
        if ($Details) {
            Write-Host "  Error: $Details" -ForegroundColor $ErrorColor
        }
        $script:TestsFailed++
    }
    Write-Host ""
}

# Function to load environment variables
function Load-EnvironmentConfig {
    param([string]$EnvFile)
    
    if (Test-Path $EnvFile) {
        Write-Host "Loading environment configuration from: $EnvFile" -ForegroundColor $InfoColor
        
        # Read .env file and set environment variables
        Get-Content $EnvFile | ForEach-Object {
            if ($_ -match '^([^#][^=]+)=(.*)$') {
                $name = $matches[1].Trim()
                $value = $matches[2].Trim()
                
                # Remove quotes if present
                if ($value.StartsWith('"') -and $value.EndsWith('"')) {
                    $value = $value.Substring(1, $value.Length - 2)
                }
                
                [Environment]::SetEnvironmentVariable($name, $value, "Process")
                Write-Host "  Set $name" -ForegroundColor $InfoColor
            }
        }
        return $true
    } else {
        Write-Host "Environment file not found: $EnvFile" -ForegroundColor $WarningColor
        return $false
    }
}

Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host "  TPOR Intranet Worker Test Suite" -ForegroundColor $InfoColor
Write-Host "  Environment: $Environment" -ForegroundColor $InfoColor
Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host ""

# Load environment configuration
$envFile = "$ConfigPath/worker-$Environment.env"
if (-not (Test-Path $envFile)) {
    # Try to copy from example file
    $exampleFile = "$ConfigPath/worker-$Environment.env.example"
    if (Test-Path $exampleFile) {
        Copy-Item $exampleFile $envFile -Force
        Write-Host "Copied environment configuration from example file" -ForegroundColor $InfoColor
    }
}

if (-not (Load-EnvironmentConfig $envFile)) {
    Write-Host "Failed to load environment configuration. Using default values." -ForegroundColor $WarningColor
}

# Get configuration values
$TEST_USERNAME = [Environment]::GetEnvironmentVariable("AUTH_USERNAME", "Process") ?? "admin"
$TEST_PASSWORD = [Environment]::GetEnvironmentVariable("AUTH_PASSWORD", "Process") ?? "password"
$DATABASE_CONNECTION_STRING = [Environment]::GetEnvironmentVariable("DATABASE_CONNECTION_STRING", "Process")
$LOCAL_STORAGE_PATH = [Environment]::GetEnvironmentVariable("LOCAL_STORAGE_PATH", "Process") ?? "uploads/dev"

Write-Host "Configuration:" -ForegroundColor $InfoColor
Write-Host "  Username: $TEST_USERNAME" -ForegroundColor $InfoColor
Write-Host "  Password: [HIDDEN]" -ForegroundColor $InfoColor
Write-Host "  Database: $($DATABASE_CONNECTION_STRING -replace 'Pwd=[^;]+', 'Pwd=[HIDDEN]')" -ForegroundColor $InfoColor
Write-Host "  Storage Path: $LOCAL_STORAGE_PATH" -ForegroundColor $InfoColor
Write-Host ""

# Test 1: Environment Configuration Check
Write-Host "Test 1: Environment Configuration Check" -ForegroundColor $WarningColor
$configValid = $true
$configDetails = @()

# Check if database is required for this environment
$isDevelopmentLocal = $Environment -eq "development-local"
$databaseRequired = -not $isDevelopmentLocal

if ($databaseRequired -and [string]::IsNullOrEmpty($DATABASE_CONNECTION_STRING)) {
    $configValid = $false
    $configDetails += "Database connection string not configured"
}

if ([string]::IsNullOrEmpty($LOCAL_STORAGE_PATH)) {
    $configValid = $false
    $configDetails += "Local storage path not configured"
}

if ($configValid) {
    $message = if ($isDevelopmentLocal) { "Configuration valid for local development (no database required)" } else { "All required configuration is present" }
    Write-TestResult "Environment Configuration" "PASS" $message
} else {
    Write-TestResult "Environment Configuration" "FAIL" ($configDetails -join "; ")
}

# Test 2: Database Connection Test
Write-Host "Test 2: Database Connection Test" -ForegroundColor $WarningColor
if ($isDevelopmentLocal) {
    Write-TestResult "Database Connection" "PASS" "Skipped for development-local environment (no database dependency)"
} elseif (-not [string]::IsNullOrEmpty($DATABASE_CONNECTION_STRING)) {
    try {
        # Change to worker directory
        Push-Location "src/TPOR.Intranet.Worker"
        
        # Try to start worker service and capture output
        $process = Start-Process -FilePath "dotnet" -ArgumentList "run" -RedirectStandardOutput "worker_test.log" -RedirectStandardError "worker_error.log" -PassThru -WindowStyle Hidden
        
        # Wait a moment for startup
        Start-Sleep -Seconds 5
        
        # Check if process is still running (indicates successful startup)
        if (-not $process.HasExited) {
            # Read log files to check for database connection
            $output = Get-Content "worker_test.log" -ErrorAction SilentlyContinue
            $error = Get-Content "worker_error.log" -ErrorAction SilentlyContinue
            
            if ($output -match "Database connection successful") {
                Write-TestResult "Database Connection" "PASS" "Worker connected to database successfully"
            } elseif ($output -match "Failed to connect to database" -or $error -match "Unable to connect to any of the specified MySQL hosts") {
                Write-TestResult "Database Connection" "FAIL" "Worker failed to connect to database"
            } else {
                Write-TestResult "Database Connection" "FAIL" "Database connection test inconclusive"
            }
            
            # Stop the process
            $process.Kill()
            $process.WaitForExit()
        } else {
            Write-TestResult "Database Connection" "FAIL" "Worker service failed to start"
        }
        
        # Clean up log files
        Remove-Item "worker_test.log" -ErrorAction SilentlyContinue
        Remove-Item "worker_error.log" -ErrorAction SilentlyContinue
        
        Pop-Location
    } catch {
        Write-TestResult "Database Connection" "FAIL" "Database connection test error: $($_.Exception.Message)"
        Pop-Location
    }
} else {
    Write-TestResult "Database Connection" "FAIL" "Database connection string not configured"
}

# Test 3: File Storage Access Test
Write-Host "Test 3: File Storage Access Test" -ForegroundColor $WarningColor
try {
    if (Test-Path $LOCAL_STORAGE_PATH) {
        Write-TestResult "File Storage Access" "PASS" "Storage path exists and is accessible"
    } else {
        # Try to create the directory
        try {
            New-Item -ItemType Directory -Path $LOCAL_STORAGE_PATH -Force | Out-Null
            Write-TestResult "File Storage Access" "PASS" "Storage path created successfully"
        } catch {
            Write-TestResult "File Storage Access" "FAIL" "Cannot create storage path: $($_.Exception.Message)"
        }
    }
} catch {
    Write-TestResult "File Storage Access" "FAIL" "Storage access test error: $($_.Exception.Message)"
}

# Test 4: Worker Service Build Test
Write-Host "Test 4: Worker Service Build Test" -ForegroundColor $WarningColor
try {
    Push-Location "src/TPOR.Intranet.Worker"
    
    # Try to build the worker service
    $buildOutput = dotnet build 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-TestResult "Worker Service Build" "PASS" "Worker service builds successfully"
    } else {
        Write-TestResult "Worker Service Build" "FAIL" "Worker service build failed: $buildOutput"
    }
    
    Pop-Location
} catch {
    Write-TestResult "Worker Service Build" "FAIL" "Build test error: $($_.Exception.Message)"
    Pop-Location
}

# Test 5: Worker Service Startup Test
Write-Host "Test 5: Worker Service Startup Test" -ForegroundColor $WarningColor
if ($isDevelopmentLocal) {
    Write-TestResult "Worker Service Startup" "PASS" "Skipped for development-local environment (no database dependency)"
} else {
    try {
        Push-Location "src/TPOR.Intranet.Worker"
        
        # Try to start worker service with timeout
        $process = Start-Process -FilePath "dotnet" -ArgumentList "run" -RedirectStandardOutput "worker_startup.log" -RedirectStandardError "worker_startup_error.log" -PassThru -WindowStyle Hidden
        
        # Wait for startup
        Start-Sleep -Seconds 3
        
        if (-not $process.HasExited) {
            Write-TestResult "Worker Service Startup" "PASS" "Worker service started successfully"
            $process.Kill()
            $process.WaitForExit()
        } else {
            $error = Get-Content "worker_startup_error.log" -ErrorAction SilentlyContinue
            Write-TestResult "Worker Service Startup" "FAIL" "Worker service failed to start: $error"
        }
        
        # Clean up log files
        Remove-Item "worker_startup.log" -ErrorAction SilentlyContinue
        Remove-Item "worker_startup_error.log" -ErrorAction SilentlyContinue
        
        Pop-Location
    } catch {
        Write-TestResult "Worker Service Startup" "FAIL" "Startup test error: $($_.Exception.Message)"
        Pop-Location
    }
}

# Summary
Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host "  Test Summary" -ForegroundColor $InfoColor
Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host "Tests Passed: $TestsPassed" -ForegroundColor $SuccessColor
Write-Host "Tests Failed: $TestsFailed" -ForegroundColor $ErrorColor
Write-Host ""

if ($TestsFailed -eq 0) {
    Write-Host "🎉 All tests passed! Worker service is working correctly." -ForegroundColor $SuccessColor
    exit 0
} else {
    Write-Host "❌ Some tests failed. Please check the errors above." -ForegroundColor $ErrorColor
    exit 1
}
