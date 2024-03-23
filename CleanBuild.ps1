Push-Location $PSScriptRoot
try {
  cleanup -y
  Remove-Item ".\Source\BlazorState\wwwroot\js" -Recurse -Force
  # Push-Location ".\Source\BlazorState"
  # npm install
  # Pop-Location
  dotnet build
  Get-ChildItem ".\Source\BlazorState\wwwroot\js"
}
finally {
  Pop-Location
}

