# Configuration variables
$SutProjectDir = "$PSScriptRoot/Tests/Test.App/Test.App.Server"
$OutputPath = "$PSScriptRoot/Tests/Test.App/Output"
$SutUrl = "https://localhost"
$TestProjectDir = "$PSScriptRoot/Tests/Test.App.EndToEnd.Tests"
$TestProjectPath = "$TestProjectDir/Test.App.EndToEnd.Tests.csproj"
$SutPort = 7011
$MaxRetries = 30
$RetryInterval = 1

function Build-And-Publish-Sut {
  Push-Location $SutProjectDir
  try {
    # Restore dependencies
    dotnet restore

    # Build the solution
    dotnet build --configuration Debug --no-restore

    # Publish the SUT
    dotnet publish --configuration Debug --output $OutputPath
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
  # Start the SUT in the background
  Write-Host "Starting SUT: ${OutputPath}/Test.App.Server.exe --urls ${SutUrl}:${SutPort}"
  $sutProcess = Start-Process -NoNewWindow -FilePath "${OutputPath}/Test.App.Server.exe" -ArgumentList "--urls ${SutUrl}:${SutPort}" -PassThru

  return $sutProcess
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
    $settings = @("edge.runsettings")

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
Build-And-Publish-Sut
Build-Test

$sutProcess = Start-Sut

try {
  Wait-For-Sut -url "${SutUrl}:${SutPort}" -maxRetries $MaxRetries -retryInterval $RetryInterval
  Run-Tests
}
finally {
  Kill-Sut -sutProcess $sutProcess
}
