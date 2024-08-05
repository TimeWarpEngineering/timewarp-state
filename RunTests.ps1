Push-Location $PSScriptRoot
try {
    # List files in LocalNugetFeed
    Get-ChildItem ./LocalNugetFeed
    dotnet restore
    dotnet build --project ./Tests/TimeWarp.State.Analyzer.Tests/TimeWarp.State.Analyzer.Tests.csproj
    dotnet fixie TimeWarp.State.Analyzer.Tests TimeWarp.State.Tests TimeWarp.State.Plus.Tests TimeWarp.Archtecture.Tests Client.Integration.Tests 
}
finally {
    Pop-Location
}
