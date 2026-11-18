#!/usr/bin/env pwsh
# Purpose
#   GitHub Actions gate before `Run CI Pipeline`: when a passing `dev ci` run already covered this exact tree,
#   restore its packages into artifacts/packages and set fast_path=true so the workflow skips the pipeline.
#
# Design
#   Reads timewarp-ganda 660's unsigned note on refs/notes/ganda-ci (same contract as
#   `ganda repo ci-note status --require ...`). ganda is not installed on the runner (private repo, no
#   release binary), and compiling the dev CLI here would spend the minutes this gate exists to save, so the
#   check is git plus PowerShell. Every miss prints why and exits 0: the full pipeline then runs as before.
#   Lookup is by HEAD^{tree} (RFC resolution 4): a merge commit or refs/pull/N/merge whose tree equals the
#   tree `dev ci` ran on hits; any other tree misses.
#   Drift (RFC resolution 5): the note's sdk must equal `dotnet --version` after Setup .NET, and its rid must
#   equal this runner's rid and start with linux-. playwright and chromium are recorded for the reader.
#   Packages (RFC resolution 3): refs/ci-packages/<tree> holds the *.nupkg files; the set of names and every
#   sha256 must match the note exactly, or the gate clears artifacts/packages and misses.
#   $RequiredStages matches CiNoteInput.StageIds in tools/dev-cli/endpoints/workflow/ci-note-input.cs
#   (a unit test reads this line). The package ref is unpacked through a tar file, not a pipe, so no
#   PowerShell pipeline touches the bytes.
#   A script, not a runfile: a runfile compile on a cold runner costs more than the whole fast path.
#   why: task 150, no-signing override (Steven 2026-11-17); workflow-pwsh-shell audit check
#   (no bash steps)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false

$RequiredStages = @('assert-version-ssot', 'build', 'test', 'e2e', 'verify-samples')
$NotesRef = 'refs/notes/ganda-ci'
$PackagesDir = 'artifacts/packages'

function Write-Miss([string] $Reason)
{
  if (Test-Path $PackagesDir) { Remove-Item -Recurse -Force $PackagesDir }
  Write-Host "CI fast path: miss ($Reason). Running the full pipeline."
  exit 0
}

function Invoke-Git([string[]] $Arguments)
{
  $output = & git @Arguments 2>$null
  if ($LASTEXITCODE -ne 0) { return $null }
  return ($output -join "`n").Trim()
}

$tree = Invoke-Git @('rev-parse', 'HEAD^{tree}')
if (-not $tree) { Write-Miss "cannot read HEAD's tree" }
Write-Host "CI fast path: tree $tree"

if ($null -eq (Invoke-Git @('fetch', '--no-tags', '--quiet', 'origin', "+${NotesRef}:${NotesRef}"))) { Write-Miss "no $NotesRef on origin" }
$noteText = Invoke-Git @('notes', "--ref=$NotesRef", 'show', $tree)
if (-not $noteText) { Write-Miss "no CI note for tree $tree" }
Write-Host "CI fast path: note $noteText"

try { $note = $noteText | ConvertFrom-Json -AsHashtable } catch { Write-Miss 'the CI note is not valid JSON' }
if ($note -isnot [System.Collections.IDictionary]) { Write-Miss 'the CI note is not a JSON object' }
if ($note['tree'] -cne $tree) { Write-Miss 'the note names another tree' }

$stages = $note['stages']
foreach ($stage in $RequiredStages)
{
  $value = if ($stages -is [System.Collections.IDictionary] -and $stages.Contains($stage)) { $stages[$stage] } else { 'missing' }
  if ($value -cne 'pass') { Write-Miss "stage $stage is $value" }
}

$tools = if ($note['tools'] -is [System.Collections.IDictionary]) { $note['tools'] } else { @{} }
$runnerSdk = ((& dotnet --version) -join '').Trim()
if ($LASTEXITCODE -ne 0) { Write-Miss 'dotnet --version failed' }
if ([string] $tools['sdk'] -cne $runnerSdk) { Write-Miss "SDK drift: note $($tools['sdk']), runner $runnerSdk" }

$arch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant()
$runnerRid = if ($IsLinux) { "linux-$arch" } else { "not-linux-$arch" }
$noteRid = [string] $tools['rid']
if (-not $noteRid.StartsWith('linux-', [StringComparison]::Ordinal)) { Write-Miss "the note's rid '$noteRid' is not linux" }
if ($noteRid -cne $runnerRid) { Write-Miss "RID drift: note $noteRid, runner $runnerRid" }

$packages = @($note['packages'])
if ($packages.Count -eq 0 -or $null -eq $packages[0]) { Write-Miss 'the note lists no packages' }
foreach ($package in $packages)
{
  if ($package -isnot [System.Collections.IDictionary] -or
      [string] $package['name'] -cnotmatch '^[A-Za-z0-9._-]+\.nupkg$' -or
      [string] $package['sha256'] -cnotmatch '^[0-9a-f]{64}$')
  {
    Write-Miss 'the note has a malformed package entry'
  }
}

$packagesRef = "refs/ci-packages/$tree"
if ($null -eq (Invoke-Git @('fetch', '--no-tags', '--quiet', 'origin', "+${packagesRef}:${packagesRef}"))) { Write-Miss "no $packagesRef on origin" }

if (Test-Path $PackagesDir) { Remove-Item -Recurse -Force $PackagesDir }
New-Item -ItemType Directory -Force -Path $PackagesDir | Out-Null
$tarPath = Join-Path ([System.IO.Path]::GetTempPath()) "ci-packages-$tree.tar"
if ($null -eq (Invoke-Git @('archive', '--format=tar', '-o', $tarPath, $packagesRef))) { Write-Miss "cannot archive $packagesRef" }
& tar -x -f $tarPath -C $PackagesDir
$tarExit = $LASTEXITCODE
Remove-Item -Force $tarPath
if ($tarExit -ne 0) { Write-Miss "cannot unpack $packagesRef" }

$listed = @($packages | ForEach-Object { [string] $_['name'] } | Sort-Object -CaseSensitive)
$present = @(Get-ChildItem -Force -Name $PackagesDir | Sort-Object -CaseSensitive)
if (($listed -join "`n") -cne ($present -join "`n")) { Write-Miss 'the package ref does not hold exactly the packages the note lists' }

foreach ($package in $packages)
{
  $actual = (Get-FileHash -Algorithm SHA256 -Path (Join-Path $PackagesDir $package['name'])).Hash.ToLowerInvariant()
  if ($actual -cne $package['sha256']) { Write-Miss "sha256 mismatch for $($package['name'])" }
}

Write-Host "CI fast path: hit. dev ci passed $($RequiredStages -join ', ') on this tree; $($listed.Count) package(s) restored."
if ($env:GITHUB_OUTPUT) { Add-Content -Path $env:GITHUB_OUTPUT -Value 'fast_path=true' } else { Write-Host 'fast_path=true' }
exit 0
