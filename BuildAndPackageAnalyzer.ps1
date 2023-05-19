Push-Location $PSScriptRoot
try {
    # Define the project, package, and source paths.
    $analyzerPath = "./Source/BlazorStateAnalyzer/BlazorStateAnalyzer.csproj"
    $projectPath = "./Source/BlazorState/BlazorState.csproj" # <path to your .csproj file>
    $tutorialPath = "./Samples/Tutorial/Sample/Sample.sln"
    $packageOutputPath = "./Nuget" # <path where you want the .nupkg file to go>
    $localSourcePath = "C:\LocalNugetFeed" # <path to your local NuGet source>
    $nugetConfigPath = "./Samples/Tutorial/Sample/nuget.config"
    $sourceName = "LocalFeed"

    # Ensure the local NuGet source directory exists.
    if (!(Test-Path -Path $localSourcePath)) {
        New-Item -ItemType Directory -Path $localSourcePath | Out-Null
    }

    # remove everything
    dotnet cleanup -y
    dotnet clean

    dotnet restore 

    # Build the analyzer.
    dotnet build $analyzerPath --configuration Release

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
    dotnet build $tutorialPath --configuration Release /p:UseSharedCompilation=false 
}
finally {
    Pop-Location
}
