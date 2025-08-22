# Configuration variables
$SutProjectDir = "$PSScriptRoot/tests/test-app/test-app-server"
$OutputPath = "$PSScriptRoot/tests/test-app/output"
$UseHttp = $env:UseHttp -eq "true"
$Protocol = if ($UseHttp) { "http" } else { "https" }
$SutUrl = "${Protocol}://localhost"
$TestProjectDir = "$PSScriptRoot/tests/test-app-end-to-end-tests"
$TestProjectPath = "$TestProjectDir/test-app-end-to-end-tests.csproj"
$AnalyzerProjectPath = "$PSScriptRoot/source/timewarp-state-analyzer/timewarp-state-analyzer.csproj"
$SourceGeneratorProjectPath = "$PSScriptRoot/source/timewarp-state-source-generator/timewarp-state-source-generator.csproj"
$SutPort = 7011
$MaxRetries = 30
$RetryInterval = 1
$RunMode = "Auto"  # Possible values: "Auto", "Manual", "Development", "Release"

function Write-StepHeader($stepName) {
    Write-Host "`n========== Starting: $stepName ==========" -ForegroundColor Cyan
}

function Write-StepFooter($stepName) {
    Write-Host "========== Completed: $stepName ==========`n" -ForegroundColor Green
}


function Ensure-Browsers-Installed {
  $playwrightPath = "$TestProjectDir/bin/Debug/net9.0/playwright.ps1"
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

function Update-ClientAppSettings {
    $appSettingsPath = "$PSScriptRoot/tests/test-app/test-app-client/wwwroot/appsettings.json"
    $appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
    $appSettings.UseHttp = $UseHttp
    $appSettings | ConvertTo-Json | Set-Content $appSettingsPath
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
          $env:UseHttp = $UseHttp.ToString().ToLower()
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

function Display-SutLogs {
    param (
        [string]$outputLogPath,
        [string]$errorLogPath
    )

    Write-Host "Displaying SUT logs:" -ForegroundColor Yellow

    if (Test-Path $outputLogPath) {
        Write-Host "SUT Output:" -ForegroundColor Cyan
        Get-Content $outputLogPath
    } else {
        Write-Host "SUT Output log file not found at: $outputLogPath" -ForegroundColor Red
    }
    
    if (Test-Path $errorLogPath) {
        Write-Host "SUT Error Output:" -ForegroundColor Cyan
        Get-Content $errorLogPath
    } else {
        Write-Host "SUT Error log file not found at: $errorLogPath" -ForegroundColor Red
    }
}

function Run-Tests {
    Push-Location $TestProjectDir
    try {
        $settings = @("chrome.runsettings")
        $global:testsFailed = $false

        Write-Host "Running E2E tests" -ForegroundColor Cyan
        foreach ($setting in $settings) {
            $targetArguments = @("test", "--no-build", "--settings:PlaywrightSettings\$setting", "./Test.App.EndToEnd.Tests.csproj")
            Write-Host "Executing: dotnet $($targetArguments -join ' ')" -ForegroundColor Yellow
            
            dotnet @targetArguments
            if ($LASTEXITCODE -ne 0) {
                $global:testsFailed = $true
                break
            }
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

function Install-LinuxDevCerts {
    if ($IsLinux) {
        Write-Host "Installing Linux development certificates..."
        dotnet linux-dev-certs install
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Linux development certificates installed successfully."
        } else {
            Write-Error "Failed to install Linux development certificates."
            exit 1
        }
    } else {
        Write-Host "Skipping Linux development certificate installation (not running on Linux)."
    }
}

# Main script execution
Write-StepHeader "Restore-Tools-And-Cleanup"
Restore-Tools-And-Cleanup
Write-StepFooter "Restore-Tools-And-Cleanup"

Write-StepHeader "Install-LinuxDevCerts"
Install-LinuxDevCerts
Write-StepFooter "Install-LinuxDevCerts"

Write-StepHeader "Build-Analyzer"
Build-Analyzer
Write-StepFooter "Build-Analyzer"

Write-StepHeader "Build-SourceGenerator"
Build-SourceGenerator
Write-StepFooter "Build-SourceGenerator"

Write-StepHeader "Update-ClientAppSettings"
Update-ClientAppSettings
Write-StepFooter "Update-ClientAppSettings"

Write-StepHeader "Build-And-Publish-Sut"
Build-And-Publish-Sut
Write-StepFooter "Build-And-Publish-Sut"

Write-StepHeader "Build-Test"
Build-Test
Write-StepFooter "Build-Test"

Write-StepHeader "Ensure-Browsers-Installed"
Ensure-Browsers-Installed
Write-StepFooter "Ensure-Browsers-Installed"


Write-StepHeader "Start-Sut"
$sutProcess = Start-Sut -Mode $RunMode
Write-StepFooter "Start-Sut"

try {
    Wait-For-Sut -url "${SutUrl}:${SutPort}" -maxRetries $MaxRetries -retryInterval $RetryInterval
    
    Run-Tests

    if ($RunMode -eq "Auto") {
        $outputLogPath = Join-Path $OutputPath "sut_output.log"
        $errorLogPath = Join-Path $OutputPath "sut_error.log"
        
        if ($global:testsFailed) {
            Write-Host "Tests failed. Displaying SUT logs:" -ForegroundColor Red
            Display-SutLogs -outputLogPath $outputLogPath -errorLogPath $errorLogPath
        } else {
            Write-Host "Tests passed. SUT logs available at:" -ForegroundColor Green
            Write-Host "Output log: $outputLogPath" -ForegroundColor Cyan
            Write-Host "Error log: $errorLogPath" -ForegroundColor Cyan
        }
    }

    if ($RunMode -eq "Development" -or $RunMode -eq "Release") {
        Write-Host "Tests completed. SUT is still running in $RunMode mode. Press Ctrl+C to stop."
        while ($true) { Start-Sleep -Seconds 1 }
    }
}
catch {
    Write-Host "An error occurred during test execution: $_" -ForegroundColor Red
    Write-Host "Stack trace: $($_.ScriptStackTrace)" -ForegroundColor Red
    if ($RunMode -eq "Auto") {
        $outputLogPath = Join-Path $OutputPath "sut_output.log"
        $errorLogPath = Join-Path $OutputPath "sut_error.log"
        Display-SutLogs -outputLogPath $outputLogPath -errorLogPath $errorLogPath
    }
}
finally {
    if ($RunMode -eq "Auto") {
        Kill-Sut -sutProcess $sutProcess
    } else {
        Write-Host "Please remember to stop the SUT process running in $RunMode mode."
    }
}
