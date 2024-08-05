Push-Location $PSScriptRoot
try
{
  $Configuration = "Release"
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
