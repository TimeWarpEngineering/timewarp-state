Push-Location $PSScriptRoot
try {
    dotnet fixie TimeWarp.State.Analyzer.Tests TimeWarp.State.Tests Client.Integration.Tests TimeWarp.State.Plus.Tests
}
finally {
    Pop-Location
}
