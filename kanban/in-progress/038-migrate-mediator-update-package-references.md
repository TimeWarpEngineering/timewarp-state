# Task 038: Migrate Mediator - Update Package References

## Description

- Update package references from TimeWarp.Mediator to martinothamar/Mediator
- This is the first step in migrating to the source-generator-based Mediator library

## Requirements

- Replace TimeWarp.Mediator package with Mediator.Abstractions and Mediator.SourceGenerator
- Ensure packages compile (even if with errors in dependent code)

## Checklist

### Implementation
- [x] Update `Directory.Packages.props`:
  - [x] Remove `<PackageVersion Include="TimeWarp.Mediator" Version="13.0.0" />`
  - [x] Add `<PackageVersion Include="Mediator.Abstractions" Version="3.0.1" />`
  - [x] Add `<PackageVersion Include="Mediator.SourceGenerator" Version="3.0.1">` with PrivateAssets
- [x] Update `source/timewarp-state/timewarp-state.csproj`:
  - [x] Replace TimeWarp.Mediator reference with Mediator.Abstractions
  - [x] Add Mediator.SourceGenerator reference
- [x] Update `tests/test-app/test-app-contracts/test-app-contracts.csproj`:
  - [x] Replace TimeWarp.Mediator reference with Mediator.Abstractions
- [x] Verify packages restore successfully with `dotnet restore`

### Review
- [x] Confirm package versions are compatible (using 3.0.1 - explicit version required by CPM)

## Notes

**Package configuration for SourceGenerator:**
```xml
<PackageVersion Include="Mediator.SourceGenerator" Version="3.0.*">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageVersion>
```

**Files to modify:**
1. `Directory.Packages.props`
2. `source/timewarp-state/timewarp-state.csproj`
3. `tests/test-app/test-app-contracts/test-app-contracts.csproj`

## Implementation Notes

- Used explicit version `3.0.1` instead of floating `3.0.*` because Central Package Management (CPM) doesn't allow floating versions by default (NU1011 error)
- All packages restored successfully with `dotnet restore`
