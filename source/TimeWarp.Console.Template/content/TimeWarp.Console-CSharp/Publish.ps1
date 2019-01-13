# TODO build for other environments
dotnet publish .\Source\Console-CSharp.csproj --configuration Release --runtime win10-x64


$CommandDir = $PSScriptRoot + "\bin\netcoreapp2.2\win10-x64"
if (!$env:Path.Contains($CommandDir)) { 
  $env:Path += ";" + $CommandDir
}
