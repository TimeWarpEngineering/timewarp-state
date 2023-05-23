$Env:ASPNETCORE_ENVIRONMENT = "Development"

Push-Location $PSScriptRoot
Push-Location ./Tests/TestApp/Server
try {
    dotnet run /p:UseSharedCompilation=false
}
finally {
    Pop-Location
    Pop-Location
}