Push-Location $PSScriptRoot
try 
{
  $testProjectDir = Join-Path $PSScriptRoot "Tests\Test.App.EndToEnd.Tests"
  Push-Location $testProjectDir
  
#  $settings = @("chrome.runsettings", "edge.runsettings", "webkit.runsettings")
  $settings = @("edge.runsettings")
  $snapshots = @()
  $outputDir = Join-Path $testProjectDir "Output"
  Write-Host "Output directory: $outputDir"
  
  $projectFile = "Test.App.EndToEnd.Tests.csproj"

  # The path for the merged coverage snapshot
  $mergedSnapshotPath = Join-Path $outputDir "mergedCoverage.snapshot"
  
  $detailedXmlReportPath = Join-Path $outputDir "coverage.xml"
  $htmlReportPath = Join-Path $outputDir "coverageReport.html"

  foreach ($setting in $settings) 
  {
    $snapshotName = "coverage{0}.snapshot" -f $setting.Replace(".runsettings", "")
    $snapshotPath = Join-Path $outputDir $snapshotName
    $targetArguments = "test --no-build --settings:PlaywrightSettings\$setting ./$projectFile"
    dotnet dotCover cover-dotnet .\dotcover.config.xml  --output=$snapshotPath --targetArguments=$targetArguments
    $snapshots += $snapshotPath
  }

  # Merge snapshots
  $sourceParameter = ($snapshots -join ";")
  Write-Host "Source parameter for dotCover merge: $sourceParameter"
  dotnet dotcover merge --output=$mergedSnapshotPath --Source=$sourceParameter

  # Generate DetailedXML report
  dotnet dotcover report --source=$mergedSnapshotPath --output=$detailedXmlReportPath  --reportType="DetailedXml"

  # Generate HTML report
  dotnet dotcover report --source=$mergedSnapshotPath --output=$htmlReportPath --reportType="HTML"

  # Run ReportGenerator to convert the DetailedXml report to Cobertura format using local tool
  dotnet reportgenerator "-reports:$detailedXmlReportPath" "-targetdir:$outputDir" "-reporttypes:Cobertura"
  
  # Open the HTML report - this is typically only useful when running locally
  if (-not $env:CI) {
    Start-Process $htmlReportPath
  }
}
finally {
  Pop-Location
}
