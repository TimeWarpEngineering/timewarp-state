# Update Solution to .NET 9 and All Dependencies

## Description
Update the solution to use .NET 9 and update ALL dependencies to their latest versions. This includes:
- Migrating from .NET 8 to .NET 9
- Updating all NuGet packages to latest stable versions
- Resolving any breaking changes
- Ensuring all projects build and test successfully

## Acceptance Criteria

### Phase 1: .NET 9 Migration
- [ ] Update global.json SDK version from 8.0.403 to 9.0.203
- [ ] Update TargetFramework in Directory.Build.props from net8.0 to net9.0
- [ ] Update all .csproj files to target net9.0 where applicable
- [ ] Review Microsoft's .NET 9 migration guide for breaking changes

### Phase 2: Microsoft Package Updates
- [ ] Update Microsoft.AspNetCore.Components.Web (8.0.17 → 9.0.x)
- [ ] Update Microsoft.AspNetCore.Components.WebAssembly (8.0.17 → 9.0.x)
- [ ] Update Microsoft.AspNetCore.Components.WebAssembly.DevServer (8.0.17 → 9.0.x)
- [ ] Update Microsoft.AspNetCore.Components.WebAssembly.Server (8.0.17 → 9.0.x)
- [ ] Update Microsoft.AspNetCore.Mvc.Testing (8.0.17 → 9.0.x)
- [ ] Update Microsoft.AspNetCore.TestHost (8.0.17 → 9.0.x)
- [ ] Update Microsoft.CodeAnalysis.CSharp (4.11.0 → 4.14.0)
- [ ] Update Microsoft.CodeAnalysis.Common (4.11.0 → 4.14.0)
- [ ] Update Microsoft.CodeAnalysis.CSharp.Workspaces (4.11.0 → 4.14.0)
- [ ] Update Microsoft.CodeAnalysis.Analyzers (3.3.4 → 3.11.0)
- [ ] Update Microsoft.Extensions packages to .NET 9 compatible versions
- [ ] Update Microsoft.TypeScript.MSBuild (5.6.2 → 5.9.2)

### Phase 3: Testing Framework Updates
- [ ] Update MSTest.TestAdapter (3.6.0 → 3.10.2)
- [ ] Update MSTest.TestFramework (3.6.0 → 3.10.2)
- [ ] Update Microsoft.NET.Test.Sdk (17.11.1 → 17.14.1)
- [ ] Update Microsoft.Playwright.MSTest (1.52.0 → 1.54.0)
- [ ] Update FluentAssertions (6.12.1 → 6.12.2)
- [ ] Update coverlet.collector (6.0.2 → 6.0.4)

### Phase 4: Third-Party Package Updates
- [ ] Update JetBrains.Annotations (2024.2.0 → 2024.3.0)
- [ ] Update NetArchTest.eNhancedEdition (1.4.3 → 1.4.5)
- [ ] Check for updates to TimeWarp.Mediator
- [ ] Check for updates to Blazored packages
- [ ] Check for updates to other third-party dependencies

### Phase 5: Update Central Package Management
- [ ] Update Directory.Packages.props with all new versions
- [ ] Ensure version consistency across all projects
- [ ] Remove any deprecated package references

### Phase 6: Regenerate Lock Files
- [ ] Run `dotnet restore --force-evaluate` to regenerate packages.lock.json files
- [ ] Verify all dependencies resolve correctly
- [ ] Commit updated lock files

### Phase 7: Build and Test
- [ ] Clean solution: `dotnet clean`
- [ ] Build solution: `dotnet build`
- [ ] Run unit tests (TimeWarp.State.Tests, etc.)
- [ ] Run integration tests (Client.Integration.Tests)
- [ ] Run end-to-end tests (Test.App.EndToEnd.Tests)
- [ ] Test all sample applications (Sample00-03)
- [ ] Run architecture tests (Test.App.Architecture.Tests)
- [ ] Run analyzer tests (TimeWarp.State.Analyzer.Tests)

### Phase 8: Documentation and CI/CD
- [ ] Update README if .NET 9 requirement needs documentation
- [ ] Update GitHub Actions workflows to use .NET 9
- [ ] Update any build scripts or documentation
- [ ] Verify NuGet package generation still works
- [ ] Create follow-up task for utilizing new .NET 9 features

## Notes
- Run `dotnet outdated` after updates to verify all packages are current
- Use `dotnet outdated --version-lock major` to check for major version updates
- Test incrementally after each phase to catch issues early
- Consider creating a backup branch before starting
- Document any breaking changes or workarounds needed
- .NET 9 SDK is already installed (9.0.203 available)
