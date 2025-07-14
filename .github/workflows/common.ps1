# Function to execute command and exit on error
function Invoke-CommandWithExit {
    param([string]$Command)
    Write-Host "Executing: $Command"
    Invoke-Expression $Command
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error executing: $Command"
        exit $LASTEXITCODE
    }
}

# Validate environment variables
if ([string]::IsNullOrEmpty($env:GITHUB_WORKSPACE)) {
    throw "GITHUB_WORKSPACE environment variable is not set or is empty"
}
