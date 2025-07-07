# Define the list of files to concatenate
$files = @(
    ".ai/00-confirmation.md",
    ".ai/01-user.md",
    ".ai/02-development-process.md",
    ".ai/03-environment.md",
    ".ai/04-csharp-coding-standards.md",
    ".ai/05-dotnet-conventions.md"
)

# Initialize an empty array to store the content
$content = @()

# Loop through each file
foreach ($file in $files) {
    # Read the file content and add it to the array
    $content += (Get-Content $file -Raw) # + "`n"
}

# Join all the content into a single string
$combinedContent = $content -join "`n"

# Output the combined content (you can redirect this to a file if needed)
$combinedContent

# Optionally, save to a file
$combinedContent | Out-File -FilePath ".clinerules"