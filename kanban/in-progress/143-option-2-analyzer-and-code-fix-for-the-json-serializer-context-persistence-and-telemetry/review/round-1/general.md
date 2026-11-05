# Round 1 — general
**Date:** 2026-11-06
**Scope reviewed:** branch vs `master` (HEAD 862e06d5). Focus: `state-json-serializable-analyzer.cs`, `state-json-serializable-code-fix-provider.cs`, the code-fix csproj and packing, tests, docs. Context checked: `state-snapshot-recorder.cs`, `timewarp-state-options.cs`, `timewarp-state-telemetry-options.cs`, sample 04 contexts.

## Summary

The change adds TWS0013 (Warning) and a code fix in a separate netstandard2.0 assembly, packed beside the analyzer. It meets the task and the implementer restatement: one analyzer, one code fix, 12 tests covering the trigger, no-trigger, and fix paths, the release-tracking row, `TypeInfoResolver` guidance in the persistence, telemetry, analyzer, and options docs, and no generator change. I checked these claims against the code:

- `TimeWarpStateOptions.JsonSerializerOptions` has no default `TypeInfoResolver`, and `StateSnapshotRecorder.ResolveTypeInfo` returns null without one. So the telemetry reason is correct to fire when no context exists.
- The state generator emits no `[GeneratedCode]`, so `GeneratedCodeAnalysisFlags.None` does not hide user states.
- The analyzer assembly still has only compiler references (RS1038).

Gates I re-ran: `./bin/dev build` gave 0 errors and 151 warnings. None of the warnings come from the analyzer, code-fix, or test files, and the build has no TWS0013 hits. The analyzer suite passed 63/63. `ganda repo audit` exited 0. Its two failures are Warning severity and older than this branch: `kebab-path-names`, and `kanban-landed` for claim 098.

Risk is low. Both findings concern what the rule cannot see at compile time. The bigger one is about noise: the telemetry reason ignores the opt-in `IncludeSnapshots` switch.

## Issues

### Issue 1 — Severity: suggestion
- File: source/timewarp-state-analyzer/state-json-serializable-analyzer.cs:74 (also documentation/topics/analyzers.md:117)
- Description: The telemetry reason fires for every concrete state whenever `TimeWarpStateTelemetryOptions` resolves. Snapshots are opt-in, though: `IncludeSnapshots` defaults to false (`timewarp-state-telemetry-options.cs:24`, `state-snapshot-recorder.cs:60`). Telemetry also skips the analyzer's concern entirely when `TimeWarpStateTelemetryOptions.JsonSerializerOptions` supplies its own reflection resolver. A project that uses `TimeWarp.State.Telemetry` only for spans, which is the default, gets one new warning per state on upgrade to 12.0.0-beta.17, and `TreatWarningsAsErrors` turns that into a broken build. The docs give the `.editorconfig` opt-out only for "a context in another project". They do not say the rule ignores `IncludeSnapshots`, and they do not say to suppress it when snapshots are off. The behaviour follows the agreed restatement, so this is a documentation gap, not a logic bug.
- Suggestion: In `documentation/topics/analyzers.md` (TWS0013), the analyzer readme, and the release note, state that the telemetry reason does not read `IncludeSnapshots` or the telemetry options' resolver, because both are runtime settings. Show `dotnet_diagnostic.TWS0013.severity = none` (or `suggestion`) as the opt-out for spans-only projects. Optionally add one line in `documentation/topics/telemetry.md` next to the existing TWS0013 sentence.
- Status: open

### Issue 2 — Severity: nit
- File: source/timewarp-state-analyzer-code-fixes/state-json-serializable-code-fix-provider.cs:178
- Description: "Create StateJsonContext" always emits an `internal` context. Where `AddTimeWarpState` runs in a different project from the states, the host cannot reach `StateJsonContext.Default` to set it as `TypeInfoResolver`. Two examples are a hosted WASM server and an Auto/prerender server that registers the client's states. The repo's own server sample declares its context `public` (`samples/04-telemetry/server/sample-04-server/sample-json-context.cs`).
- Suggestion: Emit `public sealed partial class`. Or keep `internal` and add one sentence to the TWS0013 docs: change the generated context to `public` when another project wires the resolver. Either is fine; update `Given_No_Context_Creates_One` if the emitted text changes.
- Status: open
