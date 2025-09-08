# Test API Service by Environment
# This script tests the TPOR.Intranet.API service based on the specified environment

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

# Function to make HTTP requests
function Invoke-TestRequest {
    param(
        [string]$Method,
        [string]$Url,
        [string]$Body = $null,
        [hashtable]$Headers = @{}
    )
    
    try {
        $params = @{
            Method = $Method
            Uri = $Url
            Headers = $Headers
        }
        
        if ($Body) {
            $params.Body = $Body
        }
        
        $response = Invoke-RestMethod @params -ErrorAction Stop
        return $response
    } catch {
        return $_.Exception.Message
    }
}

Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host "  TPOR Intranet API Test Suite" -ForegroundColor $InfoColor
Write-Host "  Environment: $Environment" -ForegroundColor $InfoColor
Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host ""

# Load environment configuration
$envFile = "$ConfigPath/api-$Environment.env"
if (-not (Test-Path $envFile)) {
    # Try to copy from example file
    $exampleFile = "$ConfigPath/api-$Environment.env.example"
    if (Test-Path $exampleFile) {
        Copy-Item $exampleFile $envFile -Force
        Write-Host "Copied environment configuration from example file" -ForegroundColor $InfoColor
    }
}

if (-not (Load-EnvironmentConfig $envFile)) {
    Write-Host "Failed to load environment configuration. Using default values." -ForegroundColor $WarningColor
}

# Get configuration values
$API_URL = [Environment]::GetEnvironmentVariable("ASPNETCORE_URLS", "Process")
if ($API_URL -like "*localhost*") {
    $API_URL = $API_URL -replace ".*localhost:(\d+).*", "http://localhost:`$1"
} else {
    $API_URL = "http://localhost:5001"  # Default fallback
}

$TEST_USERNAME = [Environment]::GetEnvironmentVariable("AUTH_USERNAME", "Process") ?? "admin"
$TEST_PASSWORD = [Environment]::GetEnvironmentVariable("AUTH_PASSWORD", "Process") ?? "password"

Write-Host "Configuration:" -ForegroundColor $InfoColor
Write-Host "  API URL: $API_URL" -ForegroundColor $InfoColor
Write-Host "  Username: $TEST_USERNAME" -ForegroundColor $InfoColor
Write-Host "  Password: [HIDDEN]" -ForegroundColor $InfoColor
Write-Host ""

# Test 1: API Health Check
Write-Host "Test 1: API Health Check" -ForegroundColor $WarningColor
try {
    $response = Invoke-RestMethod -Uri "$API_URL/api/health" -Method GET
    if ($response.status -eq "Healthy") {
        Write-TestResult "API Health Check" "PASS" "API is running and healthy"
    } else {
        Write-TestResult "API Health Check" "FAIL" "API health check failed"
    }
} catch {
    Write-TestResult "API Health Check" "FAIL" "API not accessible: $($_.Exception.Message)"
}

# Test 2: Auth Controller Test Endpoint
Write-Host "Test 2: Auth Controller Configuration" -ForegroundColor $WarningColor
try {
    $response = Invoke-RestMethod -Uri "$API_URL/api/auth/test" -Method GET
    if ($response.jwtSecret -eq "CONFIGURED") {
        Write-TestResult "Auth Controller Configuration" "PASS" "JWT Secret is properly configured"
    } else {
        Write-TestResult "Auth Controller Configuration" "FAIL" "JWT Secret not configured"
    }
} catch {
    Write-TestResult "Auth Controller Configuration" "FAIL" "Auth controller not accessible: $($_.Exception.Message)"
}

# Test 3: JWT Token Generation
Write-Host "Test 3: JWT Token Generation" -ForegroundColor $WarningColor
$JWT_TOKEN = $null
try {
    $loginData = @{
        username = $TEST_USERNAME
        password = $TEST_PASSWORD
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$API_URL/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    
    if ($response.token) {
        $JWT_TOKEN = $response.token
        Write-TestResult "JWT Token Generation" "PASS" "Token generated successfully"
        Write-Host "  Token: $($JWT_TOKEN.Substring(0, [Math]::Min(50, $JWT_TOKEN.Length)))..." -ForegroundColor $InfoColor
    } else {
        Write-TestResult "JWT Token Generation" "FAIL" "Token generation failed"
    }
} catch {
    Write-TestResult "JWT Token Generation" "FAIL" "Login failed: $($_.Exception.Message)"
}

# Test 4: JWT Token Validation
Write-Host "Test 4: JWT Token Validation" -ForegroundColor $WarningColor
if ($JWT_TOKEN) {
    try {
        $headers = @{
            "Authorization" = "Bearer $JWT_TOKEN"
        }
        $response = Invoke-RestMethod -Uri "$API_URL/api/auth/test" -Method GET -Headers $headers
        Write-TestResult "JWT Token Validation" "PASS" "Token is valid and accepted"
    } catch {
        Write-TestResult "JWT Token Validation" "FAIL" "Token validation failed: $($_.Exception.Message)"
    }
} else {
    Write-TestResult "JWT Token Validation" "FAIL" "No token available for validation"
}

# Test 5: Create Test File
Write-Host "Test 5: Create Test File" -ForegroundColor $WarningColor
$timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$TEST_FILE_NAME = "CUSTOMER001_PROJECT001_TESTER001_LOT001_WAFER001_PROGRAM001_$timestamp.zip"

try {
    # Create test content
    "This is a test file content for TPOR Intranet API" | Out-File -FilePath "test_content.txt" -Encoding UTF8
    
    # Create zip file (using PowerShell 5.1+ Compress-Archive)
    Compress-Archive -Path "test_content.txt" -DestinationPath $TEST_FILE_NAME -Force
    Remove-Item "test_content.txt" -Force
    
    if (Test-Path $TEST_FILE_NAME) {
        Write-TestResult "Test File Creation" "PASS" "Test file created: $TEST_FILE_NAME"
    } else {
        Write-TestResult "Test File Creation" "FAIL" "Failed to create test file"
    }
} catch {
    Write-TestResult "Test File Creation" "FAIL" "Error creating test file: $($_.Exception.Message)"
}

# Test 6: File Upload with Authentication
Write-Host "Test 6: File Upload with Authentication" -ForegroundColor $WarningColor
if ($JWT_TOKEN -and (Test-Path $TEST_FILE_NAME)) {
    try {
        $headers = @{
            "Authorization" = "Bearer $JWT_TOKEN"
        }
        
        # Create multipart form data for file upload
        $boundary = [System.Guid]::NewGuid().ToString()
        $LF = "`r`n"
        
        $fileBytes = [System.IO.File]::ReadAllBytes($TEST_FILE_NAME)
        $fileEnc = [System.Text.Encoding]::GetEncoding('UTF-8').GetString($fileBytes)
        
        $bodyLines = (
            "--$boundary",
            "Content-Disposition: form-data; name=`"file`"; filename=`"$TEST_FILE_NAME`"",
            "Content-Type: application/zip$LF",
            $fileEnc,
            "--$boundary--$LF"
        ) -join $LF
        
        $response = Invoke-RestMethod -Uri "$API_URL/api/fileupload/upload" -Method POST -Body $bodyLines -ContentType "multipart/form-data; boundary=$boundary" -Headers $headers
        
        if ($response.message -eq "File uploaded successfully") {
            Write-TestResult "File Upload" "PASS" "File uploaded successfully with authentication"
        } else {
            Write-TestResult "File Upload" "FAIL" "File upload failed: $($response | ConvertTo-Json)"
        }
    } catch {
        Write-TestResult "File Upload" "FAIL" "File upload error: $($_.Exception.Message)"
    }
} else {
    Write-TestResult "File Upload" "FAIL" "No token or test file available"
}

# Test 7: Clean up test files
Write-Host "Test 7: Cleanup" -ForegroundColor $WarningColor
if (Test-Path $TEST_FILE_NAME) {
    Remove-Item $TEST_FILE_NAME -Force
    Write-TestResult "Cleanup" "PASS" "Test files cleaned up"
} else {
    Write-TestResult "Cleanup" "PASS" "No test files to clean up"
}

# Summary
Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host "  Test Summary" -ForegroundColor $InfoColor
Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host "Tests Passed: $TestsPassed" -ForegroundColor $SuccessColor
Write-Host "Tests Failed: $TestsFailed" -ForegroundColor $ErrorColor
Write-Host ""

if ($TestsFailed -eq 0) {
    Write-Host "🎉 All tests passed! API service is working correctly." -ForegroundColor $SuccessColor
    exit 0
} else {
    Write-Host "❌ Some tests failed. Please check the errors above." -ForegroundColor $ErrorColor
    exit 1
}
