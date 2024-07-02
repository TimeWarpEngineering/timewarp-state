param(
  [string]$SutProjectPath = "./Tests/Test.App/Test.App.Server/Test.App.Server.csproj",
  [string]$OutputPath = "./Tests/Test.App/Output",
  [string]$SutUrl = "https://localhost"
)

function Get-AvailablePort {
  $listener = [System.Net.Sockets.TcpListener]::Create(0)
  $listener.Start()
  $port = $listener.LocalEndpoint.Port
  $listener.Stop()
  return $port
}

function Wait-ForSut {
  param(
    [string]$Url,
    [int]$Timeout = 30
  )

  $Elapsed = 0
  while ($true) {
    try {
      $Response = Invoke-WebRequest -Uri $Url -UseBasicParsing -SkipCertificateCheck
      if ($Response.StatusCode -eq 200) {
        Write-Host "SUT is ready."
        return
      }
    } catch {
      # Ignore the error and keep trying
    }

    Start-Sleep -Seconds 1
    $Elapsed += 1

    if ($Elapsed -ge $Timeout) {
      Write-Error "SUT did not start in time."
      exit 1
    }
  }
}

try {
  # Restore dependencies
  dotnet restore

  # Build the solution
  dotnet build --configuration Release --no-restore

  # Publish the SUT
  dotnet publish $SutProjectPath --configuration Release --output $OutputPath

  # Extract the project name from the SutProjectPath to determine the executable name
  $ProjectName = [System.IO.Path]::GetFileNameWithoutExtension($SutProjectPath)
  $ExecutablePath = "$OutputPath/$ProjectName.exe"

  # Get an available port
  $Port = Get-AvailablePort
  $FullSutUrl = "${SutUrl}:$Port"

  # Set environment variable for the port
  $env:SutPort = $Port  
  Write-Host "SUT URL: $FullSutUrl"
  
  # Start the SUT and capture the process ID
  $sutProcess = Start-Process -NoNewWindow -FilePath $ExecutablePath -ArgumentList "--urls $FullSutUrl" -PassThru

  # Wait for the SUT to be ready
  Wait-ForSut -Url $FullSutUrl

  # Run the tests
  ./RunE2ETests.ps1 -BaseUrl $FullSutUrl
} finally {
  # Ensure the SUT process is killed
  if ($sutProcess -and !$sutProcess.HasExited) {
    $sutProcess.Kill()
    $sutProcess | Out-Null
    Write-Host "SUT process terminated."
  }
}
