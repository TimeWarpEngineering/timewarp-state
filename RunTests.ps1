Push-Location $PSScriptRoot
try {
    dotnet fixie *.Tests
}
finally {
    Pop-Location
}