Push-Location $PSScriptRoot
try {
    dotnet cleanup -y
    Push-Location ./Source/TimeWarp.State.Analyzer/
    # dotnet clean --configuration Release
    dotnet build --configuration Release
    Pop-Location
    Push-Location ./Source/TimeWarp.State.SourceGenerator/
    # dotnet clean --configuration Release
    dotnet build --configuration Release
    Pop-Location
    Push-Location ./Source/TimeWarp.State/
    # dotnet clean --configuration Release
    dotnet build --configuration Release
    # dotnet pack --configuration Release
    Pop-Location
    Push-Location ./Source/TimeWarp.State.Plus/
    # dotnet clean --configuration Release
    dotnet build --configuration Release
    # dotnet pack --configuration Release
    Pop-Location
}
finally {
  Pop-Location
}