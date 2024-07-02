# Configuration variables
$SutProjectPath = "$PSScriptRoot/Tests/Test.App/Test.App.Server/Test.App.Server.csproj"
$OutputPath = "$PSScriptRoot/Tests/Test.App/Output"
$SutUrl = "https://localhost"
$DotCoverConfigPath = "$PSScriptRoot/Tests/Test.App.EndToEnd.Tests/dotcover.config.xml"

# Ensure these values align with the dotcover.config.xml file
# <TargetExecutable>../Test.App/Output/Test.App.Server.exe</TargetExecutable>
# <TargetArguments>--urls https://localhost:7011</TargetArguments>

Push-Location $PSScriptRoot
try {
  # Restore dependencies
  dotnet restore $SutProjectPath

  # Build the solution
  dotnet build $SutProjectPath --configuration Debug --no-restore

  # Publish the SUT
  dotnet publish $SutProjectPath --configuration Debug --output $OutputPath

  # Extract the project name from the SutProjectPath to determine the executable name
  $ProjectName = [System.IO.Path]::GetFileNameWithoutExtension($SutProjectPath)
  $ExecutablePath = "$OutputPath/$ProjectName.exe"

  # Set environment variable for the port
  $env:SutPort = 7011  # This should align with the port specified in the dotcover.config.xml

  $testProjectDir = "$PSScriptRoot/Tests/Test.App.EndToEnd.Tests"
  Push-Location $testProjectDir
  try {
    $settings = @("edge.runsettings")
    $snapshots = @()
    $outputDir = "$testProjectDir/Output"
    Write-Host "Output directory: $outputDir"

    $projectFile = "Test.App.EndToEnd.Tests.csproj"

    # Build the test project
    Write-Host "Building the test project..."
    dotnet build $projectFile --configuration Debug

    # Start the SUT with dotCover in the background
    Write-Host "Using dotCover config at: $DotCoverConfigPath"
    $sutProcess = Start-Process -NoNewWindow -FilePath "dotnet" -ArgumentList "dotCover cover $DotCoverConfigPath" -PassThru

    # Ensure the SUT process is killed in the finally block
    try {
      # Wait a bit to ensure the SUT has started
      Start-Sleep -Seconds 10

      # Run the E2E tests
      foreach ($setting in $settings)
      {
        $snapshotName = "coverage{0}.snapshot" -f $setting.Replace(".runsettings", "")
        $snapshotPath = Join-Path $outputDir $snapshotName
        $targetArguments = "test --no-build --settings:PlaywrightSettings\$setting ./$projectFile"
        dotnet dotCover cover-dotnet .\dotcover.config.xml --output=$snapshotPath --targetArguments=$targetArguments
        $snapshots += $snapshotPath
      }

      # Merge snapshots
      $mergedSnapshotPath = Join-Path $outputDir "mergedCoverage.snapshot"
      $sourceParameter = ($snapshots -join ";")
      Write-Host "Source parameter for dotCover merge: $sourceParameter"
      dotnet dotcover merge --output=$mergedSnapshotPath --Source=$sourceParameter

      # Generate DetailedXML report
      $detailedXmlReportPath = Join-Path $outputDir "coverage.xml"
      dotnet dotcover report --source=$mergedSnapshotPath --output=$detailedXmlReportPath --reportType="DetailedXml"

      # Generate HTML report
      $htmlReportPath = Join-Path $outputDir "coverage.html"
      dotnet dotcover report --source=$mergedSnapshotPath --output=$htmlReportPath --reportType="HTML"

      # Run ReportGenerator to convert the DetailedXml report to Cobertura format using local tool
      dotnet reportgenerator "-reports:$detailedXmlReportPath" "-targetdir:$outputDir" "-reporttypes:Cobertura"

      # Open the HTML report - this is typically only useful when running locally
      if (-not $env:CI) {
        Start-Process $htmlReportPath
      }
    }
    finally {
      if ($sutProcess -and !$sutProcess.HasExited) {
        $sutProcess.Kill()
        $sutProcess | Out-Null
        Write-Host "SUT process terminated."
      }
    }
  }
  finally {
    Pop-Location
  }
}
finally {
  Pop-Location
}
