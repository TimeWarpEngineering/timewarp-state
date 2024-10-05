# Configuration variables
$SutProjectDir = "$PSScriptRoot/Tests/Test.App/Test.App.Server"
$OutputPath = "$PSScriptRoot/Tests/Test.App/Output"
$SutUrl = "http://localhost"
$TestProjectDir = "$PSScriptRoot/Tests/Test.App.EndToEnd.Tests"
$TestProjectPath = "$TestProjectDir/Test.App.EndToEnd.Tests.csproj"
$AnalyzerProjectPath = "$PSScriptRoot/Source/TimeWarp.State.Analyzer/TimeWarp.State.Analyzer.csproj"
$SourceGeneratorProjectPath = "$PSScriptRoot/Source/TimeWarp.State.SourceGenerator/TimeWarp.State.SourceGenerator.csproj"
$SutPort = 7011
$MaxRetries = 30
$RetryInterval = 1
$RunMode = "Auto"  # Possible values: "Auto", "Manual", "Development", "Release"

function Setup-DeveloperCertificate {
  Write-Host "Setting up ASP.NET Core developer certificate..."
  
  # Check if the certificate exists and is valid
  dotnet dev-certs https --check --quiet
  
  if ($LASTEXITCODE -ne 0) {
    Write-Host "Developer certificate not found or not valid. Creating a new one..."
    
    # Clean any existing certificates
    dotnet dev-certs https --clean --quiet
    
    # Create and trust the developer certificate
    dotnet dev-certs https --trust --quiet
    
    if ($LASTEXITCODE -eq 0) {
      Write-Host "Developer certificate created and trusted successfully."
    } else {
      Write-Error "Failed to create or trust the developer certificate."
      exit 1
    }
  } else {
    Write-Host "Valid developer certificate already exists."
  }
}

function Ensure-Browsers-Installed {
  $playwrightPath = "$TestProjectDir/bin/Debug/net8.0/playwright.ps1"
  if (Test-Path $playwrightPath) {
    Write-Host "Installing Playwright browsers..."
    & $playwrightPath install --with-deps
  } else {
    Write-Error "Playwright script not found at $playwrightPath. Make sure the Test.App.EndToEnd.Tests project is built."
    exit 1
  }
}

function Restore-Tools-And-Cleanup {
  Push-Location $PSScriptRoot
  try {
    # Restore .NET tools
    dotnet tool restore

    # Clean the solution
    dotnet clean -y

    # Clean the Output directory
    if (Test-Path $OutputPath) {
      Remove-Item -Recurse -Force $OutputPath
      Write-Host "Deleted: $OutputPath"
    }
  }
  finally {
    Pop-Location
  }
}

function Build-Analyzer {
  Push-Location (Split-Path $AnalyzerProjectPath)
  try {
    # Build the Analyzer project
    dotnet build --configuration Release
  }
  finally {
    Pop-Location
  }
}

function Build-SourceGenerator {
  Push-Location (Split-Path $SourceGeneratorProjectPath)
  try {
    # Build the Source Generator project
    dotnet build --configuration Release
  }
  finally {
    Pop-Location
  }
}

function Build-And-Publish-Sut {
  Push-Location $SutProjectDir
  try {
    # Restore dependencies
    dotnet restore

    # Build the solution
    dotnet build --configuration Release --no-restore

    # Publish the SUT
    dotnet publish --configuration Release --output $OutputPath
  }
  finally {
    Pop-Location
  }
}

function Build-Test {
  Push-Location $TestProjectDir
  try {
    # Restore dependencies
    dotnet restore

    # Build the test project
    dotnet build --configuration Debug
  }
  finally {
    Pop-Location
  }
}

function Start-Sut {
  param (
    [string]$Mode
  )

  switch ($Mode) {
    "Manual" {
      Write-Host "Please start the SUT in another console using the following command:"
      Write-Host "${OutputPath}/Test.App.Server.exe --urls ${SutUrl}:${SutPort}"
      Write-Host "Press Enter when the SUT is ready..."
      Read-Host | Out-Null
      return $null
    }
    "Development" {
      $Env:ASPNETCORE_ENVIRONMENT = "Development"
      Write-Host "Starting SUT in Development mode..."
      Push-Location $SutProjectDir
      try {
        Start-Process pwsh -ArgumentList "-Command", "dotnet run --urls ${SutUrl}:${SutPort}" -NoNewWindow
      }
      finally {
        Pop-Location
      }
      return $null
    }
    "Release" {
      $Env:ASPNETCORE_ENVIRONMENT = "Development"
      Write-Host "Starting SUT in Release configuration..."
      Push-Location $SutProjectDir
      try {
        Start-Process pwsh -ArgumentList "-Command", "dotnet run --configuration Release --urls ${SutUrl}:${SutPort}" -NoNewWindow
      }
      finally {
        Pop-Location
      }
      return $null
    }
    default {
      # Auto mode
      Write-Host "Starting SUT in Auto mode..."
      $Env:ASPNETCORE_ENVIRONMENT = "Development"
      Write-Host "ASPNETCORE_ENVIRONMENT set to: $Env:ASPNETCORE_ENVIRONMENT"
      Write-Host "Changing directory to: $OutputPath"
      Push-Location $OutputPath
      try {
        Write-Host "Current directory: $(Get-Location)"
        Write-Host "Current directory contents:"
        Get-ChildItem | ForEach-Object { Write-Host $_.Name }
        
        if ($IsWindows) {
          $executableName = "Test.App.Server.exe"
        } else {
          $executableName = "Test.App.Server"
        }
        
        $executablePath = Join-Path $OutputPath $executableName
        
        if (Test-Path $executablePath) {
          $outputLogPath = Join-Path $OutputPath "sut_output.log"
          $errorLogPath = Join-Path $OutputPath "sut_error.log"
          Write-Host "Starting SUT: $executablePath --urls ${SutUrl}:${SutPort}"
          Write-Host "Output log: $outputLogPath"
          Write-Host "Error log: $errorLogPath"
          $sutProcess = Start-Process -NoNewWindow -FilePath $executablePath -ArgumentList "--urls", "${SutUrl}:${SutPort}" -PassThru -RedirectStandardOutput $outputLogPath -RedirectStandardError $errorLogPath
          return $sutProcess
        } else {
          Write-Error "Executable not found at $executablePath"
          exit 1
        }
      }
      catch {
        Write-Host "An error occurred: $_"
        Write-Host "Stack trace: $($_.ScriptStackTrace)"
      }
      finally {
        Pop-Location
      }
    }
  }
}

function Wait-For-Sut {
  param (
    [string]$url,
    [int]$maxRetries,
    [int]$retryInterval
  )

  $retries = 0
  while ($retries -lt $maxRetries) {
    try {
      $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 5
      if ($response.StatusCode -eq 200) {
        Write-Host "SUT is ready."
        return $true
      }
    } catch {
      Write-Host "Attempt $($retries + 1) failed: $_"
    }

    Start-Sleep -Seconds $retryInterval
    $retries++
  }

  Write-Error "SUT did not become ready in time."
  exit 1
}

function Run-Tests {
  Push-Location $TestProjectDir
  try {
    $settings = @("chrome.runsettings")

    Write-Host "Running E2E tests"
    foreach ($setting in $settings) {
      $targetArguments = @("--no-build", "--settings:PlaywrightSettings\$setting", "./Test.App.EndToEnd.Tests.csproj")
      dotnet test $targetArguments
    }
  }
  finally {
    Pop-Location
  }
}

function Kill-Sut {
  param (
    [Parameter(Mandatory=$true)]
    [System.Diagnostics.Process]$sutProcess
  )

  if ($sutProcess -and !$sutProcess.HasExited) {
    $sutProcess.Kill()
    $sutProcess | Out-Null
    Write-Host "SUT process terminated."
  }
}

# Main script execution
Restore-Tools-And-Cleanup
Build-Analyzer
Build-SourceGenerator
Build-And-Publish-Sut
Build-Test

Ensure-Browsers-Installed

Setup-DeveloperCertificate

$sutProcess = Start-Sut -Mode $RunMode

try {
  Wait-For-Sut -url "${SutUrl}:${SutPort}" -maxRetries $MaxRetries -retryInterval $RetryInterval
  if ($RunMode -eq "Auto") {
    $outputLogPath = Join-Path $OutputPath "sut_output.log"
    $errorLogPath = Join-Path $OutputPath "sut_error.log"
    
    Write-Host "Attempting to read SUT Output from: $outputLogPath"
    if (Test-Path $outputLogPath) {
      Write-Host "SUT Output:"
      Get-Content $outputLogPath
    } else {
      Write-Host "SUT Output log file not found."
    }
    
    Write-Host "Attempting to read SUT Error Output from: $errorLogPath"
    if (Test-Path $errorLogPath) {
      Write-Host "SUT Error Output:"
      Get-Content $errorLogPath
    } else {
      Write-Host "SUT Error log file not found."
    }
  }
  Run-Tests

  if ($RunMode -eq "Development" -or $RunMode -eq "Release") {
    Write-Host "Tests completed. SUT is still running in $RunMode mode. Press Ctrl+C to stop."
    while ($true) { Start-Sleep -Seconds 1 }
  }
}
catch {
  Write-Host "An error occurred during test execution: $_"
  Write-Host "Stack trace: $($_.ScriptStackTrace)"
}
finally {
  if ($RunMode -eq "Auto") {
    Kill-Sut -sutProcess $sutProcess
  } else {
    Write-Host "Please remember to stop the SUT process running in $RunMode mode."
  }
}
