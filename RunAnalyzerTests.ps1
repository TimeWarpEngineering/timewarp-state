Push-Location $PSScriptRoot
try {
    dotnet tool restore
    dotnet cleanup -y
    dotnet build ./Tests/BlazorStateAnalyzerTest/BlazorStateAnalyzerTest.csproj /p:UseSharedCompilation=false
}
finally {
    Pop-Location
}