# Migrate to Kebab-Case Naming Convention

## Description
Implement the kebab-case file naming convention as defined in ADR-0013 from the timewarp-flow repository. All file and directory names should use kebab-case format (lowercase letters, digits, hyphens for word separation) to ensure consistency, cross-platform compatibility, and alignment with modern web practices.

## Acceptance Criteria

### Phase 1: Tool Integration
- [ ] Add TimeWarp Source Generator with Roslyn analyzer for kebab-case validation
- [ ] Ensure analyzer validates that .cs files follow kebab-case naming
- [ ] Ensure analyzer validates class names match file names (kebab-case to PascalCase conversion)

### Phase 2: Core Source Migration
- [ ] Rename Source directory structure to kebab-case
- [ ] Rename all .cs files in Source to kebab-case
- [ ] Update all namespace declarations and using statements
- [ ] Update project file references

### Phase 3: Tests Migration
- [ ] Rename Tests directory structure to kebab-case
- [ ] Rename all test .cs files to kebab-case
- [ ] Update test project references

### Phase 4: Samples Migration
- [ ] Rename Samples directories to kebab-case
- [ ] Rename all sample .cs files to kebab-case
- [ ] Update sample project references

### Phase 5: Configuration Files
- [ ] Rename Directory.Build.props → directory.build.props
- [ ] Rename Directory.Packages.props → directory.packages.props
- [ ] Update all references to configuration files

### Phase 6: Scripts and Documentation
- [ ] Rename Scripts directory to scripts
- [ ] Rename Kanban directory to kanban
- [ ] Update all documentation references

### Phase 7: Validation
- [ ] Run full build: `dotnet build`
- [ ] Run all tests: `dotnet test`
- [ ] Verify NuGet package creation
- [ ] Test all sample applications
- [ ] Run analyzer to verify all files comply with convention

## Technical Details

### Naming Convention Rules
- **Format**: Lowercase letters (a-z), digits (0-9), hyphens (-) for word separation
- **No**: Leading/trailing/consecutive hyphens
- **Maximum**: 32 characters per name
- **Class Mapping**: `user-service.cs` → `public class UserService`

### Examples of Transformations
- `UserService.cs` → `user-service.cs`
- `IStateStore.cs` → `i-state-store.cs`
- `ReduxDevToolsInterop.cs` → `redux-dev-tools-interop.cs`
- `TimeWarpStateComponent.cs` → `time-warp-state-component.cs`

## Notes
- Reference: ADR-0013 from timewarp-flow repository
- Approximately 316 C# and project files need renaming
- Use `git mv` to preserve history
- Test on both Windows and Linux environments