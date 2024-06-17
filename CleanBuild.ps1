Push-Location $PSScriptRoot
try {
  cleanup -y
  Remove-Item ".\Source\TimeWarpState\wwwroot\js" -Recurse -Force
  # Push-Location ".\Source\TimeWarpState"
  # npm install
  # Pop-Location
  dotnet build
  Get-ChildItem ".\Source\TimeWarpState\wwwroot\js"
}
finally {
  Pop-Location
}

