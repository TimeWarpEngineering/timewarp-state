# Task 038: Migrate Mediator - Update Package References

## Description

- Update package references from TimeWarp.Mediator to martinothamar/Mediator
- This is the first step in migrating to the source-generator-based Mediator library

## Requirements

- Replace TimeWarp.Mediator package with Mediator.Abstractions and Mediator.SourceGenerator
- Ensure packages compile (even if with errors in dependent code)

## Checklist

### Implementation
- [ ] Update `Directory.Packages.props`:
  - [ ] Remove `<PackageVersion Include="TimeWarp.Mediator" Version="13.0.0" />`
  - [ ] Add `<PackageVersion Include="Mediator.Abstractions" Version="3.0.*" />`
  - [ ] Add `<PackageVersion Include="Mediator.SourceGenerator" Version="3.0.*">` with PrivateAssets
- [ ] Update `source/timewarp-state/timewarp-state.csproj`:
  - [ ] Replace TimeWarp.Mediator reference with Mediator.Abstractions
  - [ ] Add Mediator.SourceGenerator reference
- [ ] Update `tests/test-app/test-app-contracts/test-app-contracts.csproj`:
  - [ ] Replace TimeWarp.Mediator reference with Mediator.Abstractions
- [ ] Verify packages restore successfully with `dotnet restore`

### Review
- [ ] Confirm package versions are compatible

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

