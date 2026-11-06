# Round 1 — merged findings
**Date:** 2026-11-06
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-analyzer/state-json-serializable-analyzer.cs:74 (also documentation/topics/analyzers.md:117)
- Description: The telemetry reason fires on every concrete state whenever the project references `TimeWarp.State.Telemetry`. It does this even though snapshots are opt-in (`IncludeSnapshots` defaults to false) and even when the telemetry options supply their own resolver. A spans-only project gets one new warning per state on upgrade, and `TreatWarningsAsErrors` breaks its build. The docs offer the `.editorconfig` opt-out only for the "context in another project" case.
- Suggestion: Document that the telemetry reason cannot see `IncludeSnapshots` or the telemetry options' resolver, because both are runtime settings. Give the `dotnet_diagnostic.TWS0013.severity = none` (or `suggestion`) opt-out for spans-only projects. Put this in analyzers.md (TWS0013), the analyzer readme, and the 12.0.0-beta.17 release note, and optionally in telemetry.md.
- Source: general
- Disposition notes: fixed 2026-11-06 (fix pass). The spans-only limit and the `.editorconfig` opt-out (`none` or `suggestion`) are now in analyzers.md (TWS0013), the analyzer readme, the beta.17 release note, and telemetry.md.

### M2 — Severity: nit — Status: fixed
- File: source/timewarp-state-analyzer-code-fixes/state-json-serializable-code-fix-provider.cs:178
- Description: "Create StateJsonContext" always emits `internal`. A host project that wires `TypeInfoResolver` for another project's states cannot reach it. Examples are a hosted WASM server and an Auto/prerender server. Sample 04 server uses a `public` context.
- Suggestion: Emit `public`, or document that the generated context should be made `public` when another project wires the resolver. Update `Given_No_Context_Creates_One` if the emitted text changes.
- Source: general
- Disposition notes: fixed 2026-11-06 (fix pass). "Create StateJsonContext" now emits `public` when the state and every containing type are public, and `internal` otherwise. A public context that exposes `JsonTypeInfo` for an internal state would not compile. `Given_No_Context_Creates_One` now expects `public`. New test `Given_No_Context_And_Internal_State_Creates_Internal_One` covers the internal case. The docs state the rule.

## Duplicates / conflicts

- None (single reviewer).

## Plan alignment

- The implementer restatement matches the original task. The restatement narrows persistence to "fires only once the project declares a context", because persistence falls back to the reflection resolver. That narrowing is a documented design decision consistent with the task's intent, not a misread. No `## Learnings` entry was needed.
