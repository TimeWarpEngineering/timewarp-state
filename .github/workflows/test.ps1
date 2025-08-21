# Store the original location
Push-Location

try {
    # Source common functions
    . $PSScriptRoot/common.ps1

    # Build and Run Tests
    Set-Location $env:GITHUB_WORKSPACE
  Invoke-CommandWithExit "dotnet build ./tests/timewarp-state-analyzer-tests/timewarp-state-analyzer-tests.csproj"
  Invoke-CommandWithExit "dotnet fixie timewarp-state-analyzer-tests"

  Invoke-CommandWithExit "dotnet build ./tests/timewarp-state-tests/timewarp-state-tests.csproj"
  Invoke-CommandWithExit "dotnet fixie timewarp-state-tests"

  Invoke-CommandWithExit "dotnet build ./tests/timewarp-state-plus-tests/timewarp-state-plus-tests.csproj"
  Invoke-CommandWithExit "dotnet fixie TimeWarp.State.Plus.Tests"

  Invoke-CommandWithExit "dotnet build ./tests/client-integration-tests/client-integration-tests.csproj"
  Invoke-CommandWithExit "dotnet fixie Client.Integration.Tests"

  Invoke-CommandWithExit "dotnet build ./tests/test-app-architecture-tests/test-app-architecture-tests.csproj"
  Invoke-CommandWithExit "dotnet fixie test-app-architecture-tests"
}
finally {
  # Return to the original location
  Pop-Location
}
