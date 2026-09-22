# State.Initialize test-assembly guard is case-sensitive; kebab-cased test assemblies fail it

## Description

`source/timewarp-state/state/state.cs` (around line 59) decides whether the calling assembly is a
test assembly with `assembly.FullName.Contains("Test")` — ordinal, case-sensitive. A test
project whose assembly name follows the kebab-case house rule (`web-spa-integration-tests`,
lowercase "tests") fails the check, and the state initialization path that is supposed to be
test-only throws (`FieldAccessException` seen in timewarp-architecture).

timewarp-architecture has carried `<AssemblyName>web-spa-integration-Tests</AssemblyName>` as a
workaround since task 058 and expected 12.0.0-beta.3 to remove the need for it. It did not; the
guard is unchanged on master at 12.0.0-beta.4. Architecture task 058-001 is blocked on removing
that override until this ships.

## Requirements

- Match test assemblies case-insensitively (`Contains("test", StringComparison.OrdinalIgnoreCase)`)
  — or better, stop inferring from the assembly name at all. Preferred: an explicit opt-in the
  test host sets (e.g. `State.Initialize(allowTestAccess: true)` / a static
  `StateTestOptions.Enable()` called by the test fixture) with the name sniff kept only as a
  fallback for one more beta, then removed. Name sniffing is agreement-by-memory; the explicit
  switch is the compiler-checked form. Record the choice in the file's Design region.
- Unit test: an assembly named `something-tests` (lowercase) is treated as a test assembly;
  `something.Tests` still is; a non-test name still is not.
- Note in the release notes that consumers can drop `AssemblyName` overrides.
- Version bump (next 12.0.0-beta).

## Checklist

- [x] Guard case-insensitive, or replaced by explicit opt-in with name sniff as fallback
- [x] Tests for lowercase kebab, PascalCase, and non-test names
- [x] Design region updated
- [x] Release note line; version bump

## Session

- Created: 2026-09-22 (architecture cockpit, from 058-001 implementer finding)
- Implemented: 2026-09-22 — `StateTestOptions.Enable()` opt-in; ordinal-ignore-case name sniff as beta.5 fallback; tests; release notes; version 12.0.0-beta.5

## Notes

Consumer evidence: timewarp-architecture `tests/container-apps/web/web-spa-integration-tests/*.csproj`
line 18 `<AssemblyName>web-spa-integration-Tests</AssemblyName>` with the 058 comment; 058-001
rescope (2026-09-22) says "if State still trips on the kebab name, file it on timewarp-state, do not
keep the override".

## Results

`ThrowIfNotTestAssembly` now honors `StateTestOptions.Enable()` first, then falls back to
`Contains("test", StringComparison.OrdinalIgnoreCase)`. Design regions document that the sniff
is removed after beta.5. Unit tests cover kebab lowercase, PascalCase `.Tests`, non-test rejection,
and the explicit opt-in. Package version and release notes are at 12.0.0-beta.5; consumers can drop
`AssemblyName` overrides that only capitalized `Test`.

### How to validate

- Smoke: `dotnet test tests/timewarp-state-tests/timewarp-state-tests.csproj --nologo`
- Expect: all tests pass (including `ThrowIfNotTestAssemblyTests.Should_` kebab / PascalCase / non-test / Enable cases); `msbuild/repository.props` and `source/Directory.Build.props` show `12.0.0-beta.5`.
