Push-Location $PSScriptRoot
try {
    dotnet restore
    dotnet build --project ./Tests/TimeWarp.State.Analyzer.Tests/TimeWarp.State.Analyzer.Tests.csproj
    dotnet fixie TimeWarp.State.Analyzer.Tests TimeWarp.State.Tests Client.Integration.Tests TimeWarp.State.Plus.Tests
}
finally {
    Pop-Location
}
