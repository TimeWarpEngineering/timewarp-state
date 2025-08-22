<#
.SYNOPSIS
This script converts a Git commit timestamp to a human-readable date format and is intended for use in build processes.

.DESCRIPTION
The script takes a Git commit timestamp as input and converts it into a standardized date format ("yyyy-MM-ddTHH:mm:ssK"). It's designed to be run as part of an automated build process, specifically within MSBuild tasks, to generate and use date information from Git commits.

.NOTES
The script is intended to be called from an MSBuild task, but can be used in any context where a Git commit timestamp needs to be converted to a human-readable date.

.EXAMPLE
pwsh -NoProfile -ExecutionPolicy Bypass -File "ConvertTimestamp.ps1" -GitCommitTimestamp <timestamp>

This example shows how to call the script from an MSBuild task, passing a Git commit timestamp as an argument. The `-NoProfile` and `-ExecutionPolicy Bypass` flags ensure a clean and unrestricted execution environment.

.AUTHOR
Steven T. Cramer
#>

# Assuming GitCommitTimestamp is passed as an argument to the script
param(
  [int64]$GitCommitTimestamp
)

# Convert GitCommitTimestamp to a human-readable date
$epochStart = New-Object System.DateTime(1970, 1, 1, 0, 0, 0, [System.DateTimeKind]::Utc)
$commitDate = $epochStart.AddSeconds($GitCommitTimestamp)
$formattedDate = $commitDate.ToString("yyyy-MM-ddTHH:mm:ssK")

# Output or use the formatted date as needed
Write-Output $formattedDate
