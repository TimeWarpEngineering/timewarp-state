Push-Location $PSScriptRoot
try
{
  Get-Process dotnet | Stop-Process -ErrorAction SilentlyContinue
  dotnet nuget locals all --clear
  $Configuration = "Release"
  dotnet tool restore
  dotnet cleanup -y
  Remove-Item "./LocalNugetFeed" -Recurse -Force -ErrorAction SilentlyContinue
  Remove-Item "./Source/TimeWarp.State/wwwroot\js" -Recurse -Force -ErrorAction SilentlyContinue
  New-Item -ItemType Directory -Force -Path "./LocalNugetFeed"

  $projects = @(
    "./Source/TimeWarp.State.Analyzer/",
    "./Source/TimeWarp.State.SourceGenerator/",
    "./Source/TimeWarp.State/",
    "./Source/TimeWarp.State.Plus/",
    "./Source/TimeWarp.State.Policies/"
  )

  foreach ($project in $projects) {
    Push-Location $project
    dotnet build --configuration $Configuration
    Pop-Location
  }

  Write-Host "Projects have been built successfully"
}
finally
{
  Pop-Location
}
