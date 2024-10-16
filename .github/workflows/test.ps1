# Store the original location
Push-Location

try {
  # Validate environment variables
  if ([string]::IsNullOrEmpty($env:GITHUB_WORKSPACE)) {
    throw "GITHUB_WORKSPACE environment variable is not set or is empty"
  }

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

  # Build and Run Tests
  Set-Location $env:GITHUB_WORKSPACE
  Invoke-CommandWithExit "dotnet build ./Tests/TimeWarp.State.Analyzer.Tests/TimeWarp.State.Analyzer.Tests.csproj"
  Invoke-CommandWithExit "dotnet fixie TimeWarp.State.Analyzer.Tests"

  Invoke-CommandWithExit "dotnet build ./Tests/TimeWarp.State.Tests/TimeWarp.State.Tests.csproj"
  Invoke-CommandWithExit "dotnet fixie TimeWarp.State.Tests"

  Invoke-CommandWithExit "dotnet build ./Tests/TimeWarp.State.Plus.Tests/TimeWarp.State.Plus.Tests.csproj"
  Invoke-CommandWithExit "dotnet fixie TimeWarp.State.Plus.Tests"

  Invoke-CommandWithExit "dotnet build ./Tests/Client.Integration.Tests/Client.Integration.Tests.csproj"
  Invoke-CommandWithExit "dotnet fixie Client.Integration.Tests"

  Invoke-CommandWithExit "dotnet build ./Tests/Test.App.Architecture.Tests/Test.App.Architecture.Tests.csproj"
  Invoke-CommandWithExit "dotnet fixie Test.App.Architecture.Tests"
}
finally {
  # Return to the original location
  Pop-Location
}
