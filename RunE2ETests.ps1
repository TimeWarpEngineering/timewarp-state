Push-Location $PSScriptRoot
try {
  Push-Location .\Tests\Test.App.EndToEnd.Tests
  dotnet test --settings:chrome.runsettings
  dotnet test --settings:edge.runsettings
  dotnet test --settings:firefox.runsettings
  dotnet test --settings:webkit.runsettings
}
finally {
  Pop-Location
}
