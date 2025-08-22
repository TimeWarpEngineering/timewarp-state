Push-Location $PSScriptRoot
try
{
  Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -ErrorAction SilentlyContinue
  dotnet nuget locals all --clear
  $Configuration = "Release"
  dotnet tool restore
  dotnet cleanup -y
  Remove-Item "./LocalNugetFeed" -Recurse -Force -ErrorAction SilentlyContinue
  Remove-Item "./source/timewarp-state/wwwroot\js" -Recurse -Force -ErrorAction SilentlyContinue
  New-Item -ItemType Directory -Force -Path "./LocalNugetFeed"

  $projects = @(
    "./source/timewarp-state-analyzer/",
    "./source/timewarp-state-source-generator/",
    "./source/timewarp-state/",
    "./source/timewarp-state-plus/",
    "./source/timewarp-state-policies/"
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
