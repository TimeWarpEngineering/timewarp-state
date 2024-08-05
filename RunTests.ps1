Push-Location $PSScriptRoot
try {
    # List files in LocalNugetFeed
    Get-ChildItem ./LocalNugetFeed
    dotnet restore
    dotnet build --project ./Tests/TimeWarp.State.Analyzer.Tests/TimeWarp.State.Analyzer.Tests.csproj
    dotnet fixie TimeWarp.State.Analyzer.Tests TimeWarp.State.Tests TimeWarp.State.Plus.Tests Client.Integration.Tests Test.App.Architecture.Tests
}
finally {
    Pop-Location
}
