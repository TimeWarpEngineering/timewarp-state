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

# Restore Dotnet Tools
Invoke-CommandWithExit "dotnet tool restore"
Invoke-CommandWithExit "dotnet cleanup -y"

# Create LocalNugetFeed directory at root
New-Item -ItemType Directory -Force -Path ./LocalNugetFeed

# Build TimeWarp.State.Analyzer
Set-Location $env:GITHUB_WORKSPACE/Source/TimeWarp.State.Analyzer/
Invoke-CommandWithExit "dotnet build --configuration Debug"

# Build TimeWarp.State.SourceGenerator
Set-Location $env:GITHUB_WORKSPACE/Source/TimeWarp.State.SourceGenerator/
Invoke-CommandWithExit "dotnet build --configuration Debug"

# Build and Pack TimeWarp.State
Set-Location $env:GITHUB_WORKSPACE/Source/TimeWarp.State/
Invoke-CommandWithExit "dotnet build --configuration Debug"
Invoke-CommandWithExit "dotnet pack --configuration Debug"

# Build and Pack TimeWarp.State.Plus
Set-Location $env:GITHUB_WORKSPACE/Source/TimeWarp.State.Plus/
Invoke-CommandWithExit "dotnet build --configuration Debug"
Invoke-CommandWithExit "dotnet pack --configuration Debug"

# Build Tests
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
