# Configuration variables
$SutProjectDir = "$PSScriptRoot/Tests/Test.App/Test.App.Server"
$OutputPath = "$PSScriptRoot/Tests/Test.App/Output"
$SutUrl = "https://localhost"
$TestProjectDir = "$PSScriptRoot/Tests/Test.App.EndToEnd.Tests"
$TestProjectPath = "$TestProjectDir/Test.App.EndToEnd.Tests.csproj"
$AnalyzerProjectPath = "$PSScriptRoot/Source/TimeWarp.State.Analyzer/TimeWarp.State.Analyzer.csproj"
$SourceGeneratorProjectPath = "$PSScriptRoot/Source/TimeWarp.State.SourceGenerator/TimeWarp.State.SourceGenerator.csproj"
$SutPort = 7011
$MaxRetries = 30
$RetryInterval = 1
$RunMode = "Development"  # Possible values: "Auto", "Manual", "Development"

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
        Start-Process pwsh -ArgumentList "-Command", "dotnet watch run --urls ${SutUrl}:${SutPort}" -NoNewWindow
      }
      finally {
        Pop-Location
      }
      return $null
    }
    default {
      # Auto mode
      Write-Host "Starting SUT: ${OutputPath}/Test.App.Server.exe --urls ${SutUrl}:${SutPort}"
      $sutProcess = Start-Process -NoNewWindow -FilePath "${OutputPath}/Test.App.Server.exe" -ArgumentList "--urls ${SutUrl}:${SutPort}" -PassThru
      return $sutProcess
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
      $response = Invoke-WebRequest -Uri $url -UseBasicParsing -SkipCertificateCheck -TimeoutSec 5
      if ($response.StatusCode -eq 200) {
        Write-Host "SUT is ready."
        return $true
      }
    } catch {
      # Ignore the error and retry
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
if ($RunMode -eq "Development") {
  Build-Analyzer
  Build-SourceGenerator
  $sutProcess = Start-Sut -Mode $RunMode
  try {
    Wait-For-Sut -url "${SutUrl}:${SutPort}" -maxRetries $MaxRetries -retryInterval $RetryInterval
    Write-Host "SUT is running in Development mode. Press Ctrl+C to stop."
    while ($true) { Start-Sleep -Seconds 1 }
  }
  finally {
    Write-Host "Please remember to stop the SUT process running in Development mode."
  }
}
else {
  Restore-Tools-And-Cleanup
  Build-Analyzer
  Build-SourceGenerator
  Build-And-Publish-Sut
  Build-Test

  $sutProcess = Start-Sut -Mode $RunMode

  try {
    Wait-For-Sut -url "${SutUrl}:${SutPort}" -maxRetries $MaxRetries -retryInterval $RetryInterval
    Run-Tests
  }
  finally {
    if ($RunMode -eq "Auto") {
      Kill-Sut -sutProcess $sutProcess
    } else {
      Write-Host "Please remember to stop the manually started SUT process."
    }
  }
}
