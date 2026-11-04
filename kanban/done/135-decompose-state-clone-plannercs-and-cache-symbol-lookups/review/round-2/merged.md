# Round 2 — merged findings
**Date:** 2026-11-04
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-type-catalog.cs:169-171, :192-226
- Description: threading namespace match was module-scoped symbol equality, so it missed reference-pack types.
- Disposition notes: `InThreadingNamespace` uses the structural `InNamespace` walk. The test `state-clone-reference-pack-tests.cs` asserts the namespace split on the ref pack, and it expects reference copies with no TWSG002. Verified in round 2.

### M2 — Severity: bug — Status: fixed
- File: scripts/e2e.cs, scripts/e2e-build.cs, e2e-sut.cs, e2e-tests.cs, e2e-dev-certs.cs
- Description: comments were deleted to pass the 500-line audit cap.
- Disposition notes: comment and code lines were compared with the pre-task file. Every comment is restored, and the steps moved into `#:include` partials as a pure move. `dotnet build scripts/e2e.cs` succeeds. Verified in round 2.

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-type-catalog.cs:113-167; state-clone-planner.metadata.cs:43; state-clone-planner.symbols.cs:247
- Description: name checks depended on `GetTypeByMetadataName` identity (null or polyfill), and the `IsValueTuple` fallback compared namespace symbols.
- Disposition notes: `HasMetadataName` and `InNamespace` are structural, matching the pre-task semantics. Verified in round 2.

### M4 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.emit.cs:126-319
- Description: `Emit` was not split.
- Disposition notes: `Emit` now calls five writers. Shape tests pass. Verified in round 2.

### M5 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.diagnostics.cs:114-221
- Description: `Report` built eager reachability for every root.
- Disposition notes: reachability is computed lazily per reported root, and the walk is skipped when no root reported. The diagnostics match the eager version, because `reportedRoots` does not change during the uncovered pass. Verified in round 2.

### M6 — Severity: nit — Status: fixed
- File: state-clone-type-catalog.cs; tests (comments, `CorrectnessDiagnostics`)
- Description: unused members, missing empty-name guard, stale comments, misplaced helper.
- Disposition notes: all four were addressed. Verified in round 2.

## New findings

None.

## Duplicates / conflicts

- None. The round-2 observations in `general.md` are not findings: the redundant null-vs-`[]` branch, the hard-coded `net11.0`, the nested-type name match (the pre-task behavior), and the deliberate structural namespace match instead of symbol comparison.
