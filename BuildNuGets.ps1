Push-Location $PSScriptRoot
try
{
  $Configuration = "Release"
  dotnet cleanup -y
  Remove-Item "./LocalNugetFeed" -Recurse -Force
  Remove-Item "./Source/TimeWarp.State/wwwroot\js" -Recurse -Force
  Push-Location ./Source/TimeWarp.State.Analyzer/
  dotnet build --configuration $Configuration
  Pop-Location
  Push-Location ./Source/TimeWarp.State.SourceGenerator/
  dotnet build --configuration $Configuration
  Pop-Location
  Push-Location ./Source/TimeWarp.State/
  dotnet build --configuration $Configuration
  Pop-Location
  Push-Location ./Source/TimeWarp.State.Plus/
  dotnet build --configuration $Configuration
  Pop-Location
}
finally
{
  Pop-Location
}
