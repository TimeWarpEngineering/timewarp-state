$Env:ASPNETCORE_ENVIRONMENT = "Development"
$Env:UseHttp = "false"  # Set to "true" if you want to use HTTP locally

Push-Location $PSScriptRoot
try {
  Clear-Host
  # The analyzer is not directly referenced by the test app, so we need to build it first
  dotnet build ./Source/TimeWarp.State.Analyzer/TimeWarp.State.Analyzer.csproj
  dotnet build ./Source/TimeWarp.State.SourceGenerator/TimeWarp.State.SourceGenerator.csproj
  dotnet run --project ./Tests/Test.App/Test.App.Server/Test.App.Server.csproj --launch-profile http
}
finally {
  Pop-Location
}
