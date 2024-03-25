Push-Location $PSScriptRoot
try
{
  $testProjectDir = Join-Path $PSScriptRoot "Tests\Test.App.EndToEnd.Tests"
  Push-Location $testProjectDir
  $settings = @("chrome.runsettings", "edge.runsettings", "webkit.runsettings")
  $snapshots = @()
  $outputDir = Join-Path $testProjectDir "Output"
  Write-Host "Output directory: $outputDir"

  $projectFile = "Test.App.EndToEnd.Tests.csproj" # Specify your project file here
  foreach ($setting in $settings)
  {
    $snapshotName = "coverage{0}.snapshot" -f $setting.Replace(".runsettings", "")
    $snapshotPath = Join-Path $outputDir $snapshotName
    $targetArguments = "test --no-build --settings:PlaywrightSettings\$setting ./$projectFile"
    $outputPath = Join-Path $outputDir $snapshotName
    dotnet dotCover cover-dotnet .\dotcover.config.xml  --output=$outputPath --targetArguments=$targetArguments
    $snapshots += $snapshotPath
  }

  # Create a new variable for the Source with absolute paths
  $sourceParameter = ($snapshots -join ";")

  # Write out the Source parameter to console
  Write-Host "Source parameter for dotCover merge: $sourceParameter"

  $mergedSnapshotPath = Join-Path $outputDir "mergedCoverage.snapshot"
  dotnet dotcover merge --output=$mergedSnapshotPath --Source=$sourceParameter

  $reportPath = Join-Path $outputDir "coverageReport.html"
  dotnet dotcover report --source=$mergedSnapshotPath --output=$reportPath --reportType="HTML"

  # Open the report - this is typically only useful when running locally
  if (-not $env:CI)
  {
    Start-Process $reportPath
  }
}
finally
{
  Pop-Location
}
