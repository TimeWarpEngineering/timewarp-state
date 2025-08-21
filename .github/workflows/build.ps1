# Store the original location
Push-Location

try {
    # Source common functions
    . $PSScriptRoot/common.ps1

    # List installed .NET SDKs
    Write-Host "Listing installed .NET SDKs:"
    Invoke-CommandWithExit "dotnet --list-sdks"

  # Restore Dotnet Tools
  Invoke-CommandWithExit "dotnet tool restore"

  # Create LocalNugetFeed directory at root
  New-Item -ItemType Directory -Force -Path ./local-nuget-feed

  # Build TimeWarp.State.Analyzer
  Set-Location $env:GITHUB_WORKSPACE/source/timewarp-state-analyzer/
  Invoke-CommandWithExit "dotnet build --configuration Debug"

  # Build TimeWarp.State.SourceGenerator
  Set-Location $env:GITHUB_WORKSPACE/source/timewarp-state-source-generator/
  Invoke-CommandWithExit "dotnet build --configuration Debug"

  # Build and Pack TimeWarp.State
  Set-Location $env:GITHUB_WORKSPACE/source/timewarp-state/
  Invoke-CommandWithExit "dotnet build --configuration Debug"
  Invoke-CommandWithExit "dotnet pack --configuration Debug"

  # Build and Pack TimeWarp.State.Plus
  Set-Location $env:GITHUB_WORKSPACE/source/timewarp-state-plus/
  Invoke-CommandWithExit "dotnet build --configuration Debug"
  Invoke-CommandWithExit "dotnet pack --configuration Debug"
}
finally {
  # Return to the original location
  Pop-Location
}
