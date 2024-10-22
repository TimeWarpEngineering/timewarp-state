# BuildReadMe.ps1 - Concatenate multiple Markdown files into a single README.md.

# Save the current directory and ensure we return to it after execution.
try {
  Push-Location

  # Define the list of files to include in the README.md in the desired order.
  $files = @(
    "Badges.md",
    "Summary.md",
    "GettingStarted.md",
    "Installation.md",
    "Releases.md",
    "License.md",
    "Contributing.md",
    "Contact.md"
  )

  # Define the output file.
  $outputFile = "README.md"

  # Remove the existing README.md if it exists to start fresh.
  if (Test-Path $outputFile) {
    Remove-Item $outputFile
  }

  # Concatenate each file's content into the README.md.
  foreach ($file in $files) {
    if (Test-Path $file) {
      Get-Content $file | Add-Content $outputFile
      # Add an empty line between sections for better readability.
      Add-Content $outputFile "`n"
    }
    else {
      Write-Host "Warning: $file does not exist and will be skipped."
    }
  }

  Write-Host "README.md has been successfully generated."
}
catch {
  Write-Host "An error occurred: $_"
}
finally {
  # Return to the original directory.
  Pop-Location
}
