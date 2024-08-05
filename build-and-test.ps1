# Restore Dotnet Tools
dotnet tool restore
dotnet cleanup -y

# Create LocalNugetFeed directory at root
New-Item -ItemType Directory -Force -Path ./LocalNugetFeed

# Build TimeWarp.State.Analyzer
Set-Location $env:GITHUB_WORKSPACE/Source/TimeWarp.State.Analyzer/
dotnet build --configuration Debug

# Build TimeWarp.State.SourceGenerator
Set-Location $env:GITHUB_WORKSPACE/Source/TimeWarp.State.SourceGenerator/
dotnet build --configuration Debug

# Build and Pack TimeWarp.State
Set-Location $env:GITHUB_WORKSPACE/Source/TimeWarp.State/
dotnet build --configuration Debug
dotnet pack --configuration Debug

# Build and Pack TimeWarp.State.Plus
Set-Location $env:GITHUB_WORKSPACE/Source/TimeWarp.State.Plus/
dotnet build --configuration Debug
dotnet pack --configuration Debug

# Run Tests
Set-Location $env:GITHUB_WORKSPACE
./RunTests.ps1
