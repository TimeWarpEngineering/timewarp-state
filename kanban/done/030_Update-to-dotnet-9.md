# Update Solution to .NET 9 and All Dependencies

**Completed: 2025-08-20**

## Description
Update the solution to use .NET 9 and update ALL dependencies to their latest versions. This includes:
- Migrating from .NET 8 to .NET 9
- Updating all NuGet packages to latest stable versions
- Resolving any breaking changes
- Ensuring all projects build and test successfully

## Acceptance Criteria

### Phase 1: .NET 9 Migration ✅
- [x] Update global.json SDK version from 8.0.403 to 9.0.203
- [x] Update TargetFramework in Directory.Build.props from net8.0 to net9.0
- [x] Update all .csproj files to target net9.0 where applicable
- [x] Review Microsoft's .NET 9 migration guide for breaking changes

### Phase 2: Microsoft Package Updates ✅
- [x] Update Microsoft.AspNetCore.Components.Web (8.0.17 → 9.0.8)
- [x] Update Microsoft.AspNetCore.Components.WebAssembly (8.0.17 → 9.0.8)
- [x] Update Microsoft.AspNetCore.Components.WebAssembly.DevServer (8.0.17 → 9.0.8)
- [x] Update Microsoft.AspNetCore.Components.WebAssembly.Server (8.0.17 → 9.0.8)
- [x] Update Microsoft.AspNetCore.Mvc.Testing (8.0.17 → 9.0.8)
- [x] Update Microsoft.AspNetCore.TestHost (8.0.17 → 9.0.8)
- [x] Update Microsoft.CodeAnalysis.CSharp (4.11.0 → 4.14.0)
- [x] Update Microsoft.CodeAnalysis.Common (4.11.0 → 4.14.0)
- [x] Update Microsoft.CodeAnalysis.CSharp.Workspaces (4.11.0 → 4.14.0)
- [x] Update Microsoft.CodeAnalysis.Analyzers (3.3.4 → 4.14.0)
- [x] Update Microsoft.Extensions packages to .NET 9 compatible versions
- [x] Update Microsoft.TypeScript.MSBuild (5.6.2 → 5.9.2)

### Phase 3: Testing Framework Updates ✅
- [x] Update MSTest.TestAdapter (3.6.0 → 3.10.2)
- [x] Update MSTest.TestFramework (3.6.0 → 3.10.2)
- [x] Update Microsoft.NET.Test.Sdk (17.11.1 → 17.14.1)
- [x] Update Microsoft.Playwright.MSTest (1.52.0 → 1.54.0)
- [x] ~~Update FluentAssertions~~ Migrated to Shouldly 4.3.0
- [x] Update coverlet.collector (6.0.2 → 6.0.4)

### Phase 4: Third-Party Package Updates ✅
- [x] Update JetBrains.Annotations (2024.2.0 → 2025.2.0)
- [x] Update NetArchTest.eNhancedEdition (1.4.3 → 1.4.5)
- [x] Check for updates to TimeWarp.Mediator (13.0.0 - latest)
- [x] Check for updates to Blazored packages (all current)
- [x] Check for updates to other third-party dependencies

### Phase 5: Update Central Package Management ✅
- [x] Update Directory.Packages.props with all new versions
- [x] Ensure version consistency across all projects
- [x] Remove any deprecated package references (FluentAssertions removed)

### Phase 6: Regenerate Lock Files ✅
- [x] Run `dotnet restore --force-evaluate` to regenerate packages.lock.json files
- [x] Verify all dependencies resolve correctly
- [x] Commit updated lock files

### Phase 7: Build and Test ✅
- [x] Clean solution: `dotnet clean`
- [x] Build solution: `dotnet build`
- [x] Run unit tests (TimeWarp.State.Tests, etc.)
- [x] Run integration tests (Client.Integration.Tests)
- [x] Run end-to-end tests (Test.App.EndToEnd.Tests)
- [x] Test all sample applications (Sample00-03)
- [x] Run architecture tests (Test.App.Architecture.Tests)
- [x] Run analyzer tests (TimeWarp.State.Analyzer.Tests)

### Phase 8: Documentation and CI/CD ✅
- [x] Update README if .NET 9 requirement needs documentation
- [x] Update GitHub Actions workflows to use .NET 9
- [x] Update any build scripts or documentation
- [x] Verify NuGet package generation still works
- [x] Create follow-up task for utilizing new .NET 9 features

## Notes
- Run `dotnet outdated` after updates to verify all packages are current
- Use `dotnet outdated --version-lock major` to check for major version updates
- Test incrementally after each phase to catch issues early
- Consider creating a backup branch before starting
- Document any breaking changes or workarounds needed
- .NET 9 SDK is already installed (9.0.203 available)
