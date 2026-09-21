# Persistence generator: reject nested states; unique hint names; delete dead ToCamelCase

## Description

Code review 2026-06-11, finding 15 — **shrunk**. 075/080 changed emit to `Load()` + `LoadPersistentStateRequest`; nesting and hint names are **unchanged**.

`GetSemanticTarget` still records only namespace + class identifier. Nested `[PersistentState]` still emits a **top-level** `partial class {ClassName}`. Two same-named nested types still collide on `{Namespace}.{ClassName}_Persistence.g.cs`. Policies require **actions** nested in states, not nested states — nested `[PersistentState]` is **unsupported**.

`ToCamelCase` (`char.ToLower`) is **dead** (nothing calls it). Do not “fix” it — **delete** it. `PersistentStateMethod` on `ClassModel` is unused in `GenerateLoadClassCode` (runtime reads the attribute); drop it if it stays unused.

## Depends on

065

## Requirements

- Diagnostic (not nested partials) when `[PersistentState]` is on a nested class; skip emit.
- Hint name includes containing types (or skip emit after diagnostic) so `AddSource` cannot collide.
- Delete unused `ToCamelCase`. Drop unused `ClassModel` fields if unused.
- Generator tests: nested class diagnostic; two same simple names in one namespace do not crash the generator.
- Optional: drop `SG001` “Unique Hint Name” Info spam if still present.

## Out of scope

- Emitting a full containing-type chain (not a product feature)
- Persistence serializer/keys (065)
- PreRender/Server methods

## Checklist

- [ ] Nested `[PersistentState]` → diagnostic, no bad partial
- [ ] Hint names unique / no AddSource throw
- [ ] `ToCamelCase` gone
- [ ] Generator tests as above

## Session

- Created: code review 2026-06-11
- 2026-09-21: cockpit shrunk; Depends on 065. Dispatch after 065 merges.
