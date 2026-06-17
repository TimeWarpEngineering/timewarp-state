# Fix source generator nested class and culture handling

## Description

Code review 2026-06-11, finding 15 (`code-review-2026-06-11.md`).

`source/timewarp-state-source-generator/persistence-state-source-generator.cs` captures only namespace + class identifier (`GetSemanticTarget`), ignoring containing types:

- A **nested** `[PersistentState]` class emits a *top-level* `public partial class {ClassName}` under the namespace (line 81) that does not merge with the nested original — the generated `Load()` references `Sender`/`CancellationToken`/`Store` that don't exist there, breaking the build.
- Two same-named nested classes in one namespace produce identical hint names (`$"{NamespaceName}.{ClassName}_Persistence.g.cs"`, line 46) → `AddSource` throws `ArgumentException`, failing the entire generator.
- `ToCamelCase` (line 167) uses culture-sensitive `char.ToLower` for generated identifiers — on a tr-TR build machine an identifier starting with `I` camel-cases to a dotless-i, making generated output differ by machine locale.

## Fix

- Either reject nested `[PersistentState]` classes with a clear diagnostic, or emit the full containing-type chain as nested partials. Check first whether `[PersistentState]` on nested classes is an intended scenario (the analyzers/policies *require actions* to be nested in states, but states themselves are normally top-level) — if unsupported, a diagnostic is the simpler correct answer.
- Include containing types in the hint name either way.
- `char.ToLowerInvariant` in `ToCamelCase`.

## Checklist

- [ ] Decide: diagnostic vs containing-type-chain emission
- [ ] Hint name includes containing types
- [ ] ToLowerInvariant fix
- [ ] Generator tests: nested class scenario, same-name-different-container scenario
