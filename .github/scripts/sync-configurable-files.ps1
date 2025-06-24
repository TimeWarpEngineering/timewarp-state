# PowerShell script to handle syncing configurable files from a parent repository
# This script is called by the GitHub workflow

# Define script parameters at the top to avoid syntax issues
param (
    [string]$ConfigFile = "",
    [string]$GithubOutputFile = $env:GITHUB_OUTPUT,
    [string]$GithubStepSummary = $env:GITHUB_STEP_SUMMARY,
    [string]$GithubWorkspace = $env:GITHUB_WORKSPACE,
    [string]$GithubToken = $env:GITHUB_TOKEN,
    [string]$HasSyncPat = $env:HAS_SYNC_PAT
)

# Log PowerShell version for debugging purposes
Write-Host "PowerShell Version:"
Get-Host | Select-Object Version | Format-Table -AutoSize

# Function to download a file from a given URL with specified headers
function Download-File {
    param (
        [string]$url,
        [hashtable]$headers,
        [string]$file
    )
    try {
        $response = Invoke-WebRequest -Uri $url -Headers $headers -OutFile $file -UseBasicParsing -ErrorAction Stop
        if (Test-Path $file -PathType Leaf) {
            return @{ File = $file; Success = $true }
        } else {
            Remove-Item -Path $file -ErrorAction SilentlyContinue
            return @{ File = $file; Success = $false; Error = "Downloaded empty file" }
        }
    } catch {
        Remove-Item -Path $file -ErrorAction SilentlyContinue
        return @{ File = $file; Success = $false; Error = "$($_.Exception.Response.StatusCode) - $($_.Exception.Message)" }
    }
}

# GitHub API commit function removed - now available in separate script
# Use .github/scripts/commit-file-via-api.ps1 for standalone GitHub API commits

# Define the download function as a script block for job execution
$downloadFunction = {
    param ($url, $headers, $file)
    try {
        $response = Invoke-WebRequest -Uri $url -Headers $headers -OutFile $file -UseBasicParsing -ErrorAction Stop
        if (Test-Path $file -PathType Leaf) {
            return @{ File = $file; Success = $true }
        } else {
            Remove-Item -Path $file -ErrorAction SilentlyContinue
            return @{ File = $file; Success = $false; Error = "Downloaded empty file" }
        }
    } catch {
        Remove-Item -Path $file -ErrorAction SilentlyContinue
        return @{ File = $file; Success = $false; Error = "$($_.Exception.Response.StatusCode) - $($_.Exception.Message)" }
    }
}

# Validate required parameters
if (-not $GithubToken) {
    Write-Error "GitHub token is required"
    exit 1
}

if (-not $GithubWorkspace) {
    Write-Error "GitHub workspace path is required"
    exit 1
}

# Set ConfigFile to absolute path if not provided or relative
if (-not $ConfigFile) {
    $ConfigFile = Join-Path -Path $GithubWorkspace -ChildPath ".github/sync-config.yml"
} elseif (-not [System.IO.Path]::IsPathRooted($ConfigFile)) {
    $ConfigFile = Join-Path -Path $GithubWorkspace -ChildPath $ConfigFile
}

Write-Host "Loading configuration from $ConfigFile"

# Check if yq is installed, if not, install it based on the operating system
if (-not (Get-Command yq -ErrorAction SilentlyContinue)) {
    Write-Host "Installing yq for YAML parsing..."
    if ($IsWindows) {
        if (Get-Command winget -ErrorAction SilentlyContinue) {
            Write-Host "Using winget to install yq on Windows..."
            winget install -e --id MikeFarah.yq -h
            if (-not (Get-Command yq -ErrorAction SilentlyContinue)) {
                Write-Error "Failed to install yq using winget"
                exit 1
            }
        } else {
            Write-Error "winget not found on Windows. Please install winget to proceed with yq installation."
            exit 1
        }
    } elseif ($IsLinux) {
        $yqVersion = "v4.44.1"  # Use a specific version for consistency
        $yqUrl = "https://github.com/mikefarah/yq/releases/download/$yqVersion/yq_linux_amd64"
        $yqChecksumUrl = "https://github.com/mikefarah/yq/releases/download/$yqVersion/checksums.txt"
        $yqPath = "$env:HOME/.local/bin/yq"  # Use user-writable location to avoid sudo
        $yqDir = Split-Path -Path $yqPath -Parent
        
        if (-not (Test-Path $yqDir)) {
            New-Item -ItemType Directory -Path $yqDir | Out-Null
        }
        
        Write-Host "Downloading yq from $yqUrl..."
        Invoke-WebRequest -Uri $yqUrl -OutFile $yqPath -UseBasicParsing
        
        Write-Host "Downloading checksums from $yqChecksumUrl..."
        $tempPath = if ($env:TEMP) { $env:TEMP } else { "/tmp" }
        $checksumFile = Join-Path -Path $tempPath -ChildPath "checksums.txt"
        Invoke-WebRequest -Uri $yqChecksumUrl -OutFile $checksumFile -UseBasicParsing
        
        if (Test-Path $checksumFile) {
            $expectedChecksum = (Get-Content $checksumFile | Select-String -Pattern "yq_linux_amd64").ToString().Split()[0]
            $actualChecksum = (Get-FileHash -Path $yqPath -Algorithm SHA256).Hash.ToLower()
            
            if ($actualChecksum -ne $expectedChecksum) {
                Write-Error "Checksum verification failed for yq. Expected hash (first 8 chars): $($expectedChecksum.Substring(0,8))..., Actual hash (first 8 chars): $($actualChecksum.Substring(0,8))..."
                Remove-Item -Path $yqPath -Force -ErrorAction SilentlyContinue
                Remove-Item -Path $checksumFile -Force -ErrorAction SilentlyContinue
                exit 1
            } else {
                Write-Host "Checksum verification passed for yq. Hash (first 8 chars): $($actualChecksum.Substring(0,8))..."
            }
            Remove-Item -Path $checksumFile -Force -ErrorAction SilentlyContinue
        } else {
            Write-Warning "Could not download checksums for yq. Proceeding without verification (not recommended)."
        }
        
        if (Test-Path $yqPath) {
            & chmod +x $yqPath
            $env:PATH = "$env:PATH:$yqDir"
        } else {
            Write-Error "Failed to download yq for Linux"
            exit 1
        }
    } else {
        Write-Error "Unsupported operating system for yq installation"
        exit 1
    }
}

# Check if config file exists
if (-not (Test-Path $ConfigFile)) {
    Write-Error "Error: Configuration file $ConfigFile not found. It is required for this workflow."
    exit 1
}

# Read configuration from file
try {
    $reposCount = & yq eval '.repos | length' $ConfigFile 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error: Failed to parse configuration file. Please verify YAML syntax in $ConfigFile"
        exit 1
    }
} catch {
    Write-Error "Error: Failed to read configuration file: $($_.Exception.Message)"
    exit 1
}

if (-not $reposCount -or $reposCount -eq "null" -or [int]$reposCount -eq 0) {
    Write-Error "Error: No repositories configured in .repos section. Please verify your configuration syntax and ensure at least one repository is defined with valid source files. The repos-based configuration structure is required."
    exit 1
}

Write-Host "Using repos-based configuration structure"

# Get default values
$defaultRepo = & yq eval '.default_repo // "TimeWarpEngineering/timewarp-architecture"' $ConfigFile
$defaultBranch = & yq eval '.default_branch // "master"' $ConfigFile

# Get sync options
$defaultDestToSource = & yq eval '.sync_options.default_dest_to_source // true' $ConfigFile

Write-Host "Default repo: $defaultRepo"
Write-Host "Default branch: $defaultBranch"
Write-Host "Default dest to source: $defaultDestToSource"

# Process repositories and build file specifications
$allFileSpecs = @()

# Process each repository configuration
for ($i = 0; $i -lt [int]$reposCount; $i++) {
    $repo = & yq eval ".repos[$i].repo // `"$defaultRepo`"" $ConfigFile
    $branch = & yq eval ".repos[$i].branch // `"$defaultBranch`"" $ConfigFile
    $removePrefix = & yq eval ".repos[$i].path_transform.remove_prefix // null" $ConfigFile
    
    # Get files for this repo
    $repoFilesCount = & yq eval ".repos[$i].files | length" $ConfigFile
    
    for ($j = 0; $j -lt [int]$repoFilesCount; $j++) {
        $sourcePath = & yq eval ".repos[$i].files[$j].source_path" $ConfigFile
        $destPath = & yq eval ".repos[$i].files[$j].dest_path // null" $ConfigFile
        
        # Apply default_dest_to_source logic
        if (($destPath -eq "null" -or -not $destPath) -and $defaultDestToSource -eq "true") {
            $destPath = $sourcePath
            
            # Apply path transformation (remove prefix) if configured
            if ($removePrefix -and $removePrefix -ne "null" -and 
                $destPath -and -not [string]::IsNullOrWhiteSpace($destPath) -and 
                $destPath.StartsWith($removePrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
                
                $destPath = $destPath.Substring($removePrefix.Length)
                # Normalize path separators for cross-platform compatibility
                $destPath = $destPath.TrimStart('/', '\').Replace('/', [System.IO.Path]::DirectorySeparatorChar)
            }
        }
        
        # Validate destination path
        if (-not $destPath -or [string]::IsNullOrWhiteSpace($destPath)) {
            Write-Warning "Destination path is null or empty for source: $sourcePath. Skipping file."
            continue
        }
        
        $fileSpec = @{
            Repo = $repo
            Branch = $branch
            SourcePath = $sourcePath
            DestPath = $destPath
            RemovePrefix = $removePrefix
        }
        
        $allFileSpecs += $fileSpec
    }
}

# Filter out workflow files if no SYNC_PAT (simple approach)
if ($HasSyncPat -ne "true") {
    Write-Host "No SYNC_PAT detected - filtering out workflow files that require special permissions"
    $filteredFileSpecs = @()
    foreach ($fileSpec in $allFileSpecs) {
        # More precise pattern matching for GitHub workflow and template paths
        $isWorkflowFile = ($fileSpec.SourcePath -match '\.github[/\\]workflows[/\\]') -or 
                         ($fileSpec.SourcePath -match '\.github[/\\]workflow-templates[/\\]')
        
        if ($isWorkflowFile) {
            Write-Host "Excluding $($fileSpec.SourcePath) (requires SYNC_PAT with workflow permissions)"
        } else {
            $filteredFileSpecs += $fileSpec
        }
    }
    $allFileSpecs = $filteredFileSpecs
} else {
    Write-Host "SYNC_PAT available - will sync all files including workflows via regular file method"
}

# Get cron schedule if available
$cronSchedule = & yq eval '.schedule.cron' $ConfigFile
if ($cronSchedule -and $cronSchedule -ne "null") {
    Add-Content -Path $GithubOutputFile -Value "cron_schedule=$cronSchedule"
    Write-Host "  Cron Schedule: $cronSchedule"
} else {
    Add-Content -Path $GithubOutputFile -Value "cron_schedule=0 9 * * 1"
    Write-Host "  Cron Schedule: Using default (0 9 * * 1)"
}

# Output configuration for GitHub Actions compatibility
$primaryRepo = $defaultRepo
$primaryBranch = $defaultBranch
if ($allFileSpecs.Count -gt 0) {
    $primaryRepo = $allFileSpecs[0].Repo
    $primaryBranch = $allFileSpecs[0].Branch
}

Add-Content -Path $GithubOutputFile -Value "parent_repo=$primaryRepo"
Add-Content -Path $GithubOutputFile -Value "parent_branch=$primaryBranch"

# Create a summary of files for output
$fileSummary = ($allFileSpecs | ForEach-Object { $_.DestPath }) -join ","
Add-Content -Path $GithubOutputFile -Value "files_to_sync=$fileSummary"

Write-Host "Configuration loaded:"
Write-Host "  Primary Repository: $primaryRepo"
Write-Host "  Primary Branch: $primaryBranch"
Write-Host "  Total file specs: $($allFileSpecs.Count)"

if ($cronSchedule -and $cronSchedule -ne "null") {
    Write-Host "  Cron Schedule from config: $cronSchedule"
}

# Create temporary directory for parent repo
$tempPath = if ($env:TEMP) { $env:TEMP } else { "/tmp" }
$tempDir = Join-Path -Path $tempPath -ChildPath "parent-repo"
if (-not (Test-Path $tempDir)) {
    New-Item -ItemType Directory -Path $tempDir | Out-Null
}

# Download files from repositories
$originalLocation = Get-Location
Set-Location -Path $tempDir
$downloadedFiles = @()
$failedFiles = @()

Write-Host "Starting file downloads..."

# Use parallel jobs for downloading files
$jobs = @()

Write-Host "Processing $($allFileSpecs.Count) file specifications..."

foreach ($fileSpec in $allFileSpecs) {
    $repo = $fileSpec.Repo
    $branch = $fileSpec.Branch
    $sourcePath = $fileSpec.SourcePath
    $destPath = $fileSpec.DestPath
    
    Write-Host "Attempting to download from ${repo}@${branch}: $sourcePath -> $destPath"
    
    $fileDir = Split-Path -Path $destPath -Parent
    if ($fileDir -and -not (Test-Path $fileDir)) {
        New-Item -ItemType Directory -Path $fileDir | Out-Null
    }
    
    # Construct URL with proper encoding
    $baseUrl = "https://api.github.com/repos/$repo/contents/$sourcePath"
    $url = "$baseUrl" + "?ref=" + "$branch"
    Write-Host "API URL: $url"
    
    $headers = @{
        "Accept" = "application/vnd.github.v3.raw"
    }
    
    # Only add authorization header if token is provided and not empty
    if ($GithubToken -and $GithubToken.Trim() -ne "") {
        $headers["Authorization"] = "token $GithubToken"
    }
    
    $job = Start-Job -ScriptBlock $downloadFunction -ArgumentList $url, $headers, $destPath
    $jobs += $job
}

# Wait for all jobs to complete and collect results
foreach ($job in $jobs) {
    $result = Receive-Job -Job $job -Wait
    if ($result.Success) {
        $downloadedFiles += $result.File
        Write-Host "✓ Successfully downloaded: $($result.File)"
    } else {
        $failedFiles += $result.File
        Write-Host "✗ Failed to download: $($result.File) (Error: $($result.Error))"
    }
    Remove-Job -Job $job
}

# Output results
$env:DOWNLOADED_FILES = $downloadedFiles -join " "
$env:FAILED_FILES = $failedFiles -join " "

if ($downloadedFiles.Count -eq 0) {
    Write-Host "No files were successfully downloaded"
    exit 1
}

# Return to original location for git operations
Set-Location -Path $originalLocation

# Compare and update files
$changesMade = $false
$changedFiles = @()

foreach ($file in $downloadedFiles) {
    $sourceFile = Join-Path -Path $tempDir -ChildPath $file
    if (Test-Path $sourceFile -PathType Leaf) {
        $targetFile = Join-Path -Path $GithubWorkspace -ChildPath $file
        
        $targetDir = Split-Path -Path $targetFile -Parent
        if (-not (Test-Path $targetDir)) {
            New-Item -ItemType Directory -Path $targetDir | Out-Null
        }
        
        # Use file hash comparison to avoid loading large files into memory
        $sourceHash = Get-FileHash -Path $sourceFile -Algorithm SHA256
        if (Test-Path $targetFile) {
            $targetHash = Get-FileHash -Path $targetFile -Algorithm SHA256
            if ($sourceHash.Hash -ne $targetHash.Hash) {
                Write-Host "Updating file: $file (hash mismatch)"
                Copy-Item -Path $sourceFile -Destination $targetFile -Force
                $changedFiles += $file
                $changesMade = $true
            } else {
                Write-Host "No changes needed for: $file"
            }
        } else {
            Write-Host "Updating file: $file (target does not exist)"
            Copy-Item -Path $sourceFile -Destination $targetFile -Force
            $changedFiles += $file
            $changesMade = $true
        }
    }
}

# All files now processed via regular method - no special API handling needed

Add-Content -Path $GithubOutputFile -Value "changes_made=$changesMade"
$env:CHANGED_FILES = $changedFiles -join " "

if ($changesMade) {
    Write-Host "Files updated: $($changedFiles -join ' ')"
    
    # Stage all synced files for commit
    Write-Host "Staging synced files for commit..."
    foreach ($file in $changedFiles) {
        $targetFile = Join-Path -Path $GithubWorkspace -ChildPath $file
        if (Test-Path $targetFile) {
            Write-Host "Staging: $file"
            & git add $targetFile
        }
    }
} else {
    Write-Host "No files needed updating"
}

# Output summary
Add-Content -Path $GithubStepSummary -Value "## Sync Summary"
Add-Content -Path $GithubStepSummary -Value "**Configuration Type:** Repos-based"
Add-Content -Path $GithubStepSummary -Value "**Default Repository:** $defaultRepo"
Add-Content -Path $GithubStepSummary -Value "**Default Branch:** $defaultBranch"
Add-Content -Path $GithubStepSummary -Value "**Total File Specifications:** $($allFileSpecs.Count)"

# Create a detailed list of file specifications  
$fileSpecDetails = $allFileSpecs | ForEach-Object {
    "- **$($_.Repo)@$($_.Branch)**: $($_.SourcePath) → $($_.DestPath)"
}
Add-Content -Path $GithubStepSummary -Value "**File Specifications:**"
$fileSpecDetails | ForEach-Object { Add-Content -Path $GithubStepSummary -Value $_ }

if ($downloadedFiles) {
    Add-Content -Path $GithubStepSummary -Value "**Successfully Downloaded:** $($downloadedFiles -join ' ')"
}

if ($failedFiles) {
    Add-Content -Path $GithubStepSummary -Value "**Failed to Download:** $($failedFiles -join ' ')"
}

# Workflow-specific reporting removed - all files now use same sync method

if ($changesMade) {
    Add-Content -Path $GithubStepSummary -Value "**Files Updated:** $($changedFiles -join ' ')"
    Add-Content -Path $GithubStepSummary -Value "**Status:** ✅ Pull request created with updates"
} else {
    Add-Content -Path $GithubStepSummary -Value "**Status:** ✅ No updates needed - all files are current"
}
