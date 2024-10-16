# Store the original location
Push-Location

try {
    # Source common functions
    . $PSScriptRoot/common.ps1

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
