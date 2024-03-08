$Env:ASPNETCORE_ENVIRONMENT = "Development"

Push-Location $PSScriptRoot
try {
    dotnet build /p:UseSharedCompilation=false
    
    Push-Location ./Tests/Test.App/Test.App.Server
    try {
        dotnet run /p:UseSharedCompilation=false
    }
    finally {
        Pop-Location
    }
}
finally {
    Pop-Location
}
    