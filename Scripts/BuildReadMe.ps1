# BuildReadMe.ps1 - Concatenate multiple Markdown files into a single README.md, removing YAML front matter.

# Function to remove YAML front matter from content
function Remove-YamlFrontMatter {
    param([string]$content)
    if ($content -match "^---\s*\r?\n([\s\S]*?)\r?\n---\s*\r?\n") {
        return $content -replace "^---\s*\r?\n[\s\S]*?\r?\n---\s*\r?\n", ""
    }
    return $content
}

# Save the current directory and ensure we return to it after execution.
try {
    Push-Location

    # Define the list of files to include in the README.md in the desired order.
    $files = @(
        "../Documentation/Partials/Badges.md",
        "../Documentation/Partials/Summary.md",
        "../Documentation/Partials/GettingStarted.md",
        "../Documentation/Partials/Installation.md",
        "../Documentation/Partials/Releases.md",
        "../Documentation/Partials/License.md",
        "../Documentation/Partials/Contributing.md",
        "../Documentation/Partials/Contact.md"
    )

    # Define the output file.
    $outputFile = "README.md"

    # Remove the existing README.md if it exists to start fresh.
    if (Test-Path $outputFile) {
        Remove-Item $outputFile
    }

    # Concatenate each file's content into the README.md, removing YAML front matter.
    foreach ($file in $files) {
        if (Test-Path $file) {
            $content = Get-Content $file -Raw
            $contentWithoutFrontMatter = Remove-YamlFrontMatter $content
            $contentWithoutFrontMatter | Add-Content $outputFile
            # Add an empty line between sections for better readability.
            Add-Content $outputFile "`n"
        }
        else {
            Write-Host "Warning: $file does not exist and will be skipped."
        }
    }

    Write-Host "README.md has been successfully generated with YAML front matter removed."
}
catch {
    Write-Host "An error occurred: $_"
}
finally {
    # Return to the original directory.
    Pop-Location
}
