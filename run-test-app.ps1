$Env:ASPNETCORE_ENVIRONMENT = "Development"
$Env:UseHttp = "true"  # Default to HTTP to avoid certificate issues

Push-Location $PSScriptRoot
try {
  Clear-Host
  # The analyzer is not directly referenced by the test app, so we need to build it first
  dotnet build ./source/timewarp-state-analyzer/timewarp-state-analyzer.csproj
  dotnet build ./source/timewarp-state-source-generator/timewarp-state-source-generator.csproj
  dotnet run --project ./tests/test-app/test-app-server/test-app-server.csproj --launch-profile http
}
finally {
  Pop-Location
}
