# Round 1 — general
**Date:** 2026-09-21
**Scope reviewed:** branch `task/071-fix-source-generator-nested-class-and-culture-hand` vs `origin/master`

## Summary

The persistence generator now reports **TWSG001** and skips `AddSource` for nested `[PersistentState]` types, builds hint names with containing-type `+` segments, and deletes dead `ToCamelCase` / unused `PersistentStateMethod` / SG001 Info spam. Required behaviors are covered by a new Fixie project (4 passed); top-level emit still produces `LoadPersistentStateRequest(typeof(ClassName))` in the declaring namespace (verified via `PurpleState_Persistence.g.cs`). Overall risk is low; the only notable follow-up is incremental-pipeline hygiene around the new `Location` field.

## Issues

### Issue 1 — Severity: suggestion
- File: source/timewarp-state-source-generator/persistence-state-source-generator.cs:61
- Description: `ClassModel` stores a Roslyn `Location` (from `Identifier.GetLocation()`). `Location` retains the underlying `SyntaxTree`, which is an incremental-generator anti-pattern: it weakens caching and can pin prior compilation trees in IDE/host scenarios. `ClassModel` also lacks value equality, so the new `Location`/`HintName`/`IsNested` payload is compared by reference and re-executes more often than needed. Functional output (TWSG001 + skip emit) is correct; this is pipeline hygiene, not a product bug.
- Suggestion: Keep only equatable data in the model (e.g. file path + `TextSpan`, or a small `EquatableLocation` struct), reconstruct `Location.Create(...)` in `Execute` when reporting TWSG001, and give `ClassModel` value equality (`record` / `IEquatable`) over `NamespaceName`, `ClassName`, `IsNested`, `HintName`, and the span/path fields.
- Status: open
