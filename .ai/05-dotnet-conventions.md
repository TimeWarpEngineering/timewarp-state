.NET CONVENTIONS:

FRAMEWORK:
- Target net8.0

PROJECT CONFIGURATION:
- Use Directory.Build.props for shared project properties
- Use Directory.Packages.props for centralized package versioning
- Enable nullable reference types

SOLUTION MANAGEMENT:
- Never edit .sln file directly
  ✓ `dotnet sln add ./src/MyProject/MyProject.csproj`
  ✗ Manual .sln file editing

TOOLING:
- Initialize local tool manifest
  ✓ ```pwsh
     dotnet new tool-manifest
     ```
  Creates: .config/dotnet-tools.json
