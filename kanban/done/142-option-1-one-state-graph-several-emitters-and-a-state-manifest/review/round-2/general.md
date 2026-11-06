# Round 2 — general
**Date:** 2026-11-06
**Scope reviewed:** the round-1 fix delta, `75b10bdd..138a9298` (product paths, kanban excluded): 21 product files.
The prior M1 to M12 were re-checked against that delta.

## Summary

The fix commit addresses all twelve round-1 findings as described in their disposition notes. Each fix was checked
in the code, not only in the notes:

- `StateManifest.Parse` drops items that are not objects in `Array()`, so it never reads properties from a non-object.
- The publisher is registered through a factory.
- `InspectorManifest.SetAll` stores a batch under one lock and raises `Changed` once. The endpoint uses it.
- The analyzer csproj links the two fact files by name.
- The tests named in the notes exist and assert what the notes claim.

No new defects turned up in the product code. One stale line in the task Results was corrected.

Gates run by this reviewer in the foreground:
- `dotnet test tests/timewarp-state-analyzer-tests`: 53/53.
- `dotnet test tests/timewarp-state-devtools-tests`: 124/124.
- `dotnet test tests/timewarp-state-source-generator-tests`: 164/164.

These match the counts in Results.

Checked and not raised:
- The factory registration copies `assemblies` into an array at registration time, so a caller who mutates its
  array afterwards cannot change what gets posted. The container disposes `IDisposable` singletons that a factory
  creates.
- `SetAll` keeps `Set` semantics. A known assembly can be replaced past the cap, a new one is refused, and a replace
  counts as stored. A null element throws after earlier entries were stored, without raising `Changed`. The endpoint
  never passes null, so this edge stays theoretical.
- `Given_Struct_Action` expects CS0592 plus TWS0004. The test passes, so the attribute's `AttributeUsage` and the
  analyzer's class check agree.
- A repo-wide grep finds no leftover `NoteLogChanged`, `SyncLog` or `OnLogChanged` references.
- `Given_Every_Case_Has_A_Test` now fails on a renamed key, not only on a changed count.

## Issues

### Issue 1 — Severity: nit
- File: kanban/in-progress/142-option-1-one-state-graph-several-emitters-and-a-state-manifest/task.md (Results,
  "What changed", Analyzers bullet)
- Description: Results still say the fact files are linked with `<Compile Include=...facts\*.cs Link=...>`. Since
  the M11 fix, the analyzer csproj lists `catalog-action-facts.cs` and `state-symbol-facts.cs` by name.
- Suggestion: make Results name the two files.
- Status: fixed (the reviewer fixed it; the change is kitchen-only, in Results)
