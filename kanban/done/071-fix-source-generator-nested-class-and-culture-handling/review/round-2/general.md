# Round 2 — general
**Date:** 2026-09-21
**Scope reviewed:** M1 fix delta (`persistence-state-source-generator.cs`, `is-external-init.cs`) plus re-verify TWSG001 skip-emit

## Summary

M1 is fixed: the incremental model no longer stores Roslyn `Location`; `ClassModel` is a value-equatable `sealed record` carrying `EquatableDiagnosticLocation` (path + `TextSpan` + `LinePositionSpan`), and `Execute` reconstructs via `Location.Create`. TWSG001 still targets the nested identifier and still skips emit; generator tests remain 4 passed. No new defects in the fix delta.

## Prior findings

### M1 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-source-generator/persistence-state-source-generator.cs (ClassModel / EquatableDiagnosticLocation); source/timewarp-state-source-generator/is-external-init.cs
- Notes: Working-tree delta replaces `Location` on `ClassModel` with `EquatableDiagnosticLocation.From(Identifier.GetLocation())`; `GetLocation()` is transient in transform only. `ClassModel` and `EquatableDiagnosticLocation` are records (`sealed record` / `readonly record struct`) so pipeline equality is by value over namespace, class name, nested flag, hint name, and path/span fields. Probe after rebuild: TWSG001 on `NestedWidgetState` span `[71..88)` / text `NestedWidgetState`, `GeneratedSources=0`. `dotnet test` on `tests/timewarp-state-source-generator-tests`: Passed 4, Failed 0. `is-external-init.cs` is a minimal netstandard2.0 `IsExternalInit` polyfill enabling init-only record setters; no duplicate polyfill in the project.

## Issues
