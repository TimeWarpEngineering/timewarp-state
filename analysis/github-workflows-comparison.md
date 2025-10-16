# GitHub Workflows Comparison: TimeWarp.State vs Nuru/Amuru

## Executive Summary

TimeWarp.State has a complex, redundant workflow structure with 8 separate YAML files and PowerShell scripts, while Nuru and Amuru use a streamlined single `ci-cd.yml` approach with C# scripts. **The Nuru/Amuru methodology is significantly cleaner** and aligns with the migration goals of Task 035.

## Key Findings

### 🔴 TimeWarp.State Issues
1. **Duplicate workflows**: `ci-build.yml` and `master-build.yml` are nearly identical (99% same content)
2. **Script redundancy**: PowerShell scripts in `.github/workflows/` duplicate functionality of root scripts
3. **Technology mismatch**: Using PowerShell while migrating to C#/.NET ecosystem
4. **Maintenance burden**: 8 workflow files + 4 workflow-specific PowerShell scripts

### ✅ Nuru/Amuru Advantages
1. **Single workflow file**: One `ci-cd.yml` handles all scenarios (PR, push, release)
2. **C# scripts**: Using `dotnet run` with C# scripts (self-consistent technology)
3. **Clear separation**: Build logic in `/Scripts/*.cs`, not embedded in workflows
4. **Modern approach**: Scripts use TimeWarp.Amuru for self-bootstrapping

## Detailed Comparison

### Workflow Structure

#### TimeWarp.State (8 files)
```
.github/workflows/
├── ci-build.yml          # PR builds
├── master-build.yml       # Master branch builds (DUPLICATE of ci-build.yml)
├── release-publish.yml    # NuGet publishing
├── publish-documentation.yml # DocFX documentation
├── sync-configurable-files.yml # Parent repo sync
├── scorecard.yml         # Security scanning
├── claude.yml            # AI code review
├── build.ps1            # Build script (DUPLICATES root build-nugets.ps1)
├── test.ps1             # Test script (DUPLICATES root run-tests.ps1)
└── common.ps1           # Shared functions
```

#### Nuru/Amuru (3 files)
```
.github/workflows/
├── ci-cd.yml               # All CI/CD in one file
└── sync-configurable-files.yml # Parent repo sync (shared)

Scripts/
├── Build.cs               # C# build script
├── Clean.cs              # C# clean script
└── CheckVersion.cs       # Version validation
```

### Workflow Duplication Analysis

#### TimeWarp.State: ci-build.yml vs master-build.yml
```yaml
# ci-build.yml (lines 1-65)
name: Build and Test
on:
  pull_request:
    paths: [source/**, tests/**, ...]
  workflow_dispatch:

# master-build.yml (lines 1-68) 
name: Build and Test on Master
on:
  push:
    branches: [master]
    paths: [source/**, tests/**, ...]
  workflow_dispatch:
```
**ONLY DIFFERENCES**:
- Workflow name
- Trigger (pull_request vs push to master)
- One extra env variable in master-build.yml

**IDENTICAL**:
- All steps (build, test, e2e)
- All scripts called
- All configuration

### Script Technology Comparison

#### TimeWarp.State: PowerShell
```powershell
# .github/workflows/build.ps1
Push-Location
try {
    . $PSScriptRoot/common.ps1
    Invoke-CommandWithExit "dotnet build --configuration Debug"
    Invoke-CommandWithExit "dotnet pack --configuration Debug"
}
finally {
    Pop-Location
}
```

#### Nuru: C# Script
```csharp
#!/usr/bin/dotnet --
// Scripts/Build.cs - Uses TimeWarp.Amuru internally!

CommandResult buildResult = DotNet.Build()
    .WithProject("../TimeWarp.Nuru.slnx")
    .WithConfiguration("Release")
    .WithVerbosity("minimal")
    .Build();

ExecutionResult result = await buildResult.ExecuteAsync();
```

### Release Process Comparison

#### TimeWarp.State
- Separate `release-publish.yml` workflow
- Calls `BuildNuGets.ps1` PowerShell script
- Manual version extraction from Directory.Build.props
- 3 separate `dotnet nuget push` commands

#### Nuru/Amuru
- Integrated in `ci-cd.yml` with `release:` trigger
- C# scripts for build and version checking
- Automatic version detection via MinVer
- Single loop for all package publishing

## Redundancy Analysis

### PowerShell Script Duplication
| Root Script | Workflow Script | Functionality | Duplication Level |
|------------|-----------------|---------------|-------------------|
| `build-nugets.ps1` | `.github/workflows/build.ps1` | Build projects | ~70% overlap |
| `run-tests.ps1` | `.github/workflows/test.ps1` | Run tests | ~80% overlap |
| N/A | `.github/workflows/common.ps1` | Shared utilities | Used by workflow scripts |
| `run-e2e-tests.ps1` | Called directly | E2E testing | No duplication |

### Why the Duplication?
1. **Historical evolution**: Likely started with root scripts, added workflow-specific versions later
2. **Different contexts**: Root scripts for local dev, workflow scripts for CI
3. **Slightly different needs**: CI scripts skip some local-only steps
4. **Poor separation**: Logic embedded in scripts instead of parameterized

## Recommendations

### Immediate Actions
1. **Merge duplicate workflows**: Combine `ci-build.yml` and `master-build.yml` into single workflow
2. **Eliminate script duplication**: Use single set of scripts with parameters for CI vs local
3. **Start C# migration**: Begin converting PowerShell to C# scripts as per Task 035

### Migration Strategy

#### Phase 1: Consolidate Workflows
```yaml
# Single ci-cd.yml to replace ci-build.yml and master-build.yml
name: CI/CD
on:
  push:
    branches: [master]
  pull_request:
    branches: [master]
  release:
    types: [published]
  workflow_dispatch:

jobs:
  build-test-publish:
    # Single job with conditional steps
```

#### Phase 2: Convert to C# Scripts
Priority order based on Nuru/Amuru patterns:
1. Create `/Scripts/Build.cs` using TimeWarp.Amuru
2. Create `/Scripts/Test.cs` for test execution
3. Create `/Scripts/E2E.cs` for end-to-end tests
4. Update workflows to call C# scripts: `dotnet Scripts/Build.cs`

#### Phase 3: Align with Nuru/Amuru
- Adopt single `ci-cd.yml` pattern
- Use MinVer for automatic versioning
- Implement self-bootstrapping with TimeWarp.Amuru
- Remove all PowerShell dependencies

### Benefits of Nuru/Amuru Approach
1. **Self-consistency**: C# project using C# for build scripts
2. **Type safety**: Compile-time checking for build logic
3. **Reusability**: Scripts can use TimeWarp.Amuru internally
4. **Maintainability**: Single technology stack
5. **Cross-platform**: C# scripts work everywhere .NET does
6. **DRY principle**: No duplication between local and CI scripts

## Specific Workflow Issues

### TimeWarp.State Workflow Complexity
- **8 workflow files** for what Nuru/Amuru does in **1 file**
- **4 PowerShell scripts** in workflows directory
- **9+ PowerShell scripts** in root (with overlapping functionality)
- Total: **~20 scripts/workflows** vs Nuru's **4 files**

### Configuration Sync Workflow
Both repos use similar `sync-configurable-files.yml`, but:
- TimeWarp.State: 444-line PowerShell script
- Could be: ~100-line C# script using Octokit.NET

## Conclusion

The Nuru/Amuru approach is **definitively cleaner** and more maintainable:
- **5x fewer files** (4 vs 20+)
- **Single technology** (C# throughout)
- **No duplication** (one script per function)
- **Modern patterns** (using own tools for building)
- **Easier CI/CD** (one workflow to rule them all)

### Recommended Architecture
```
TimeWarp.State/
├── .github/
│   └── workflows/
│       ├── ci-cd.yml              # Single unified workflow
│       └── sync-configurable.yml  # Keep if needed
├── Scripts/                       # All C# scripts (NEW)
│   ├── Build.cs
│   ├── Test.cs
│   ├── E2E.cs
│   ├── Package.cs
│   └── Common.cs
└── tools/                         # Migrated PowerShell → C# apps
    ├── TimeWarp.State.Build/
    ├── TimeWarp.State.Test/
    └── TimeWarp.State.E2E/
```

This aligns with Task 035's goals and modernizes the entire build infrastructure.