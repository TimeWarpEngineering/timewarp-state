Push-Location $PSScriptRoot\..\..\..\
try {
  cleanup -y
  Remove-Item ".\Source\TimeWarp.State\wwwroot\js" -Recurse -Force
  Remove-Item ".\Source\TimeWarp.State\wwwroot\css" -Recurse -Force
  # Push-Location ".\Source\TimeWarp.State"
  # npm install
  # Pop-Location
  dotnet build
  Get-ChildItem ".\Source\TimeWarp.State\wwwroot\js"
}
finally {
  Pop-Location
}

