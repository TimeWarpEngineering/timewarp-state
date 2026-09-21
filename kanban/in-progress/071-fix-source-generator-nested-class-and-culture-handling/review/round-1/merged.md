# Round 1 — merged findings
**Date:** 2026-09-21
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-source-generator/persistence-state-source-generator.cs:61
- Description: `ClassModel` stores a Roslyn `Location` (from `Identifier.GetLocation()`). `Location` retains the underlying `SyntaxTree`, which is an incremental-generator anti-pattern: it weakens caching and can pin prior compilation trees in IDE/host scenarios. `ClassModel` also lacks value equality, so the new `Location`/`HintName`/`IsNested` payload is compared by reference and re-executes more often than needed. Functional output (TWSG001 + skip emit) is correct; this is pipeline hygiene, not a product bug.
- Suggestion: Keep only equatable data in the model (e.g. file path + `TextSpan`, or a small `EquatableLocation` struct), reconstruct `Location.Create(...)` in `Execute` when reporting TWSG001, and give `ClassModel` value equality (`record` / `IEquatable`) over `NamespaceName`, `ClassName`, `IsNested`, `HintName`, and the span/path fields.
- Source: general
- Disposition notes: `ClassModel` is a `sealed record`. Diagnostics use `EquatableDiagnosticLocation` (path + `TextSpan` + `LinePositionSpan`) and `Location.Create` in `Execute`. `is-external-init.cs` polyfills `IsExternalInit` for netstandard2.0 records. Identifier span still reported (probe: NestedWidgetState).

## Duplicates / conflicts

None — single reviewer, one issue.
