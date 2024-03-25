Push-Location $PSScriptRoot
try
{
  Push-Location .\Tests\Test.App.EndToEnd.Tests
  $settings = "chrome.runsettings", "edge.runsettings", "firefox.runsettings", "webkit.runsettings"
  foreach ($setting in $settings)
  {
    dotnet test --settings:$setting --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format="cobertura"
  }

  $coverageFiles = Get-ChildItem -Recurse -Filter "coverage.cobertura.xml"
  foreach ($coverageFile in $coverageFiles)
  {
    & reportGenerator "-reports:$coverageFile" "-targetdir:coveragereport" "-reportTypes:Html;MarkdownSummaryGithub"
  }

  .\coveragereport\index.html
}
finally
{
  Pop-Location
}
