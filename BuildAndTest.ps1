param(
  [string]$SutProjectPath = "./Tests/Test.App/Test.App.Server/Test.App.Server.csproj",
  [string]$OutputPath = "./Tests/Test.App/Output",
  [string]$SutUrl = "https://localhost:7011"
)

function Wait-ForSut {
  param(
    [string]$url,
    [int]$timeout = 30
  )

  $elapsed = 0
  while ($true) {
    try {
      $response = Invoke-WebRequest -Uri $url -UseBasicParsing -SkipCertificateCheck
      if ($response.StatusCode -eq 200) {
        Write-Host "SUT is ready."
        return
      }
    } catch {
      # Ignore the error and keep trying
    }

    Start-Sleep -Seconds 1
    $elapsed += 1

    if ($elapsed -ge $timeout) {
      Write-Error "SUT did not start in time."
      exit 1
    }
  }
}

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release --no-restore

# Publish the SUT
dotnet publish $sutProjectPath --configuration Release --output $outputPath

# Extract the project name from the SutProjectPath to determine the executable name
$ProjectName = [System.IO.Path]::GetFileNameWithoutExtension($SutProjectPath)
$ExecutablePath = "$OutputPath/$ProjectName.exe"

# Start the SUT
Start-Process -NoNewWindow -FilePath $ExecutablePath -ArgumentList "--urls $SutUrl" -PassThru

# Wait for the SUT to be ready
Wait-ForSut -url $sutUrl

# Run the tests
dotnet test --configuration Release --no-restore --logger trx
