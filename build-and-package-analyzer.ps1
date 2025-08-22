Push-Location $PSScriptRoot
try {
    # Define the project, package, and source paths.
    $analyzerPath = "./Source/TimeWarpStateAnalyzer/TimeWarp.State.Analyzer.csproj"
    $projectPath = "./Source/TimeWarpState/TimeWarp.State.csproj" # <path to your .csproj file>
    $tutorialPath = "./Samples/01-StateActionsHandlers/Sample/Sample.sln"
    $analyzerTestPath = "./Tests/TimeWarpStateAnalyzerTest/TimeWarp.State.Analyzer.Tests.csproj"
    $packageOutputPath = "./Nuget" # <path where you want the .nupkg file to go>
    $localSourcePath = "C:\LocalNugetFeed" # <path to your local NuGet source>
    $nugetConfigPath = "./Samples/01-StateActionsHandlers/Sample/nuget.config"
    $sourceName = "LocalFeed"

    # Ensure the local NuGet source directory exists.
    if (!(Test-Path -Path $localSourcePath)) {
        New-Item -ItemType Directory -Path $localSourcePath | Out-Null
    }

    # remove everything
    dotnet cleanup -y
    dotnet clean
    dotnet restore /p:UseSharedCompilation=false

    # Build the analyzer.
    dotnet build $analyzerPath --configuration Release /p:UseSharedCompilation=false

    Start-Sleep -Seconds 5

    # Build the project.
    dotnet build $projectPath --configuration Release --no-restore /p:UseSharedCompilation=false

    # Pack the project into a NuGet package.
    dotnet pack $projectPath --configuration Release --output $packageOutputPath /p:UseSharedCompilation=false

    # Get the path to the newly created .nupkg file.
    $packageName = Get-ChildItem -Path $packageOutputPath -Filter *.nupkg | Sort-Object LastWriteTime -Descending | Select-Object -First 1


    # Get the list of sources
    $sources = dotnet nuget list source --configfile $nugetConfigPath

    # Check if the source is already in the list
    if ($sources | Select-String -Pattern $sourceName -Quiet) {
        Write-Host "Source '$sourceName' already exists."
    }
    else {
        # Add the source if it's not already in the list
        dotnet nuget add source $localSourcePath --configfile $nugetConfigPath --name $sourceName
    }

    # Add the package to the local NuGet source.
    Move-Item -Path $packageName.FullName -Destination $localSourcePath -Force

    # Build the sample and make sure no compiler errors
    Write-Host "#### Building Sample should be no compiler errors."
    dotnet build $tutorialPath --configuration Release /p:UseSharedCompilation=false 

    # Build the TimeWarpStateAnalyzerTest App and we should have 3 errors
    Write-Host "#### Build the TimeWarpStateAnalyzerTest App and we should have 3 errors"
    dotnet build $analyzerTestPath --configuration Release /p:UseSharedCompilation=false

}
finally {
    Pop-Location
}
