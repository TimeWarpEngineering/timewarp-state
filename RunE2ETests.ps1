Push-Location $PSScriptRoot
try {
  Push-Location .\Tests\Test.App.EndToEnd.Tests
  $settings = @("chrome.runsettings", "edge.runsettings", "webkit.runsettings")
  $snapshots = @()

  foreach ($setting in $settings) {
    $snapshot = "coverage{0}.snapshot" -f $setting.Replace(".runsettings", "")
    # dotnet dotcover cover --output=$snapshot --DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format="cobertura" --settings=$setting --targetExecutable="dotnet" --targetArguments="test --no-build --settings:$setting"
    dotnet dotCover cover .\dotcover.config.xml --output=Output\$snapshot --targetArguments="test --no-build --settings:PlaywrightSettings\$setting"
    $snapshots += ".\Output\$snapshot"
  }

  # Create a new variable for the Source
  $sourceParameter = $snapshots -join ';'

  # Write out the Source parameter to console
  Write-Host "Source parameter for dotCover merge: $sourceParameter"
  
  $mergedSnapshot = "mergedCoverage.snapshot"
  dotnet dotcover merge --output="Output\$mergedSnapshot" --Source=$sourceParameter

  dotnet dotcover report --source=$mergedSnapshot --output="Output\coverageReport.html" --reportType="HTML"

  .\Output\coverageReport.html
}
finally {
  Pop-Location
}
