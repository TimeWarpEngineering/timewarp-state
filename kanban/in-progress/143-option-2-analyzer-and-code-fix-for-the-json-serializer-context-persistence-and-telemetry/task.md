# Option 2: analyzer and code fix for the JSON serializer context

## Description

Source: task 140 `report/analysis.md` (options as code, costs, what each unblocks). Read it first. Option 2. Independent of the others.

Where output must be seen by a generator TimeWarp does not own (System.Text.Json), use an analyzer and code fix that write into user source. The analyzer reports each state that persistence or telemetry needs but no `JsonSerializerContext` lists; the code fix adds the `[JsonSerializable]` line.

## Requirements

- One TWS analyzer, one code fix, tests for both, release tracking.
- `AddTimeWarpState` guidance for wiring the context as `TypeInfoResolver`.
- Telemetry today silently skips a state with no type info (`state-snapshot-recorder.cs`); the rule must cover that path.
- No generator change.

## Goal (implementer restatement)

System.Text.Json's source generator only sees user source, so TimeWarp cannot generate the
`[JsonSerializable]` lines for states. Ship a Roslyn analyzer (next free id, TWS0013) that finds each
concrete `State<T>` that needs JSON type info and is not listed by any `JsonSerializerContext` in the
project, plus a code fix that writes the missing `[JsonSerializable(typeof(X))]` line into user source.

"Needs JSON" means:

- **Persistence:** the state has `[PersistentState]`. Persistence falls back to the reflection resolver
  when no context exists, so this reason fires only once the project declares a context.
- **Telemetry:** the project references `TimeWarp.State.Telemetry`. Snapshots have no reflection
  fallback (`state-snapshot-recorder.cs` skips a state whose `TypeInfoResolver` returns null), so this
  reason fires whether or not a context exists. With no context, the code fix creates one.

Acceptance:

- Analyzer + code fix with tests (positive, negative, fix output) under `tests/timewarp-state-analyzer-tests`.
- Release tracking row in `AnalyzerReleases.Unshipped.md`.
- The code fix ships inside the TimeWarp.State package (`analyzers/dotnet/cs`), in its own assembly so the
  analyzer assembly keeps compiler-only references (RS1038).
- Docs: rule page/readme and `AddTimeWarpState` guidance for wiring the context as `TypeInfoResolver`
  (persistence and telemetry).
- No generator change. Build 0 warnings / 0 errors; tests and `ganda repo audit` green.

## Checklist

- [x] Analyzer and code fix with tests
- [x] Documentation updated
- [x] Release tracking (TWS0013 in `AnalyzerReleases.Unshipped.md`)
- [x] Build 0 errors; no warnings from the new or touched projects (the 151 repo-wide warnings are pre-existing, see Results)

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06)
- 2026-11-06 implement (ganda task work): analyzer, code fix, tests, docs. `ganda repo audit` initially failed only on
  `kanban-landed` (this kitchen was in-progress on the branch and to-do on origin-home), fixed with `ganda kanban reconcile`.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 9ce89c26-a711-4b1f-a78e-1e760cb71745, max-turns 500, Done — 2026-11-05T21:31:02Z
- Review oracle round 1 (ganda task work, tw-implementation-review, effort 3, roster general): claude-opus-5-5, 2026-11-06.
  Two open findings (M1 suggestion, M2 nit). Fix pass requested.
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session d83d5a56-7156-434b-b706-415fbfb6a01b, max-turns 200, artifacts general.md, merged.md — 2026-11-05T21:36:17Z

- 2026-11-06 fix pass (review round 1, implement-fix-2): fixed M1 (docs) and M2 (the code-fix context accessibility follows the
  state). Analyzer tests 64/64, `./bin/dev build` 0 errors, `./bin/dev test` green.

## Results

**TWS0013 (Warning, Design)**, `source/timewarp-state-analyzer/state-json-serializable-analyzer.cs`. It reports a concrete
`State<T>` (direct or through an abstract base) that no `JsonSerializerContext` declared in the project lists via
`[JsonSerializable(typeof(X))]`, when either reason applies:

- **persistence:** `[PersistentState]` on the state and at least one context in the project. With no context, persistence
  uses the reflection resolver, so the rule stays quiet. Sample 05 has no context and gets no new warning.
- **telemetry snapshots:** the project references `TimeWarp.State.Telemetry` (`TimeWarpStateTelemetryOptions` resolves).
  This reason fires with or without a context, because `StateSnapshotRecorder.TryCaptureJson` skips a state with no
  `JsonTypeInfo`, logging only at debug level.

Contexts are found by a lazy walk of the source assembly once per compilation. The diagnostic is therefore a plain
symbol diagnostic: the editor shows the squiggle and the light bulb without full-solution analysis. Abstract and open
generic states are skipped. A context in another project is not seen; the docs give the `.editorconfig` suppression for that case.

**Code fix:** `source/timewarp-state-analyzer-code-fixes/state-json-serializable-code-fix-provider.cs`, in a new
netstandard2.0 project. It is a separate assembly so `timewarp-state-analyzer.dll` keeps compiler-only references (RS1038).
The fix offers one action per context in the project ("Add [JsonSerializable(typeof(X))] to Ctx"). The line goes after the
context's last attribute list, and the type name is simplified against the file's usings. With no context, the fix offers
"Create StateJsonContext", which adds `state-json-context.cs` beside the state. The context is `public` when the state is
effectively public, so a host project can wire it, and `internal` otherwise. There is no FixAll provider: per-state edits
to one context declaration would race. The dll is packed into `TimeWarp.State` at
`analyzers/dotnet/cs/timewarp-state-analyzer-code-fixes.dll`, and the packaging test now asserts it. Workspaces is
`PrivateAssets=all`, and the nuspec still lists only TimeWarp.Mediator.

**Wiring:**

- Added to `timewarp-state.slnx`, with a BuildDependency on TimeWarp.State, and to the `TimeWarp.State` csproj.
- Added `Microsoft.CodeAnalysis.CSharp.Workspaces` 4.14.0 (same Roslyn floor as the analyzer) and
  `Microsoft.CodeAnalysis.CSharp.CodeFix.Testing` 1.1.4 to `Directory.Packages.props`.
- No generator changed.

**Tests:** `tests/timewarp-state-analyzer-tests/state-json-serializable-analyzer-tests.cs`, 13 cases:

- Trigger: persistence, telemetry with no context, both reasons, intermediate base.
- No trigger: listed in a nested context in another namespace, persistence with no context, no telemetry, abstract,
  generic, or foreign `State<T>`.
- Fix: append after existing attributes, add to an attribute-less context in another file and namespace (doc comment kept),
  pick the second of two contexts, create a new context file (`public` for a public state, `internal` for an internal one).

**Docs:**

- `documentation/topics/analyzers.md` (TWS0013 section).
- `source/timewarp-state-analyzer/readme.md`.
- `documentation/topics/persistence.md` (Trimming and AOT: `TypeInfoResolver` wiring in `AddTimeWarpState`).
- `documentation/topics/telemetry.md` (Snapshot type info).
- `source/timewarp-state-telemetry/readme.md`.
- Release notes 12.0.0-beta.17.
- XML docs on `TimeWarpStateOptions.JsonSerializerOptions`.

**Boy-scout:** moved the file-level `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.Operations` usings in four
analyzer files into `global-usings.cs`. That fixes the 5 TW0007 warnings, so the analyzer project now builds 0/0.

**Gates run:**

- `./bin/dev build`: 0 errors. The 151 warnings are all pre-existing in other projects (TW0007, RS0030, CS8600/8602,
  NU1510, BL0010/16, NU1902/3 from Analyzer.Testing's NuGet.Packaging), and none come from the analyzer, code-fix, or new test files.
- `./bin/dev test`: every suite green, analyzer tests 63/63.
- `./bin/dev pack` plus the packaging tests: 4/4.
- `ganda repo audit`: see Session.

### How to validate

Smoke:

```bash
./bin/dev build
(cd tests/timewarp-state-analyzer-tests && dotnet fixie --tests "StateJsonSerializableAnalyzer_*")   # or: ./bin/dev test
./bin/dev pack && unzip -l artifacts/packages/TimeWarp.State.12.0.0-beta.17.nupkg | grep analyzers/
```

Expect:

- The build has 0 errors.
- The `StateJsonSerializableAnalyzer_` tests pass (13 cases), and the whole analyzer suite reports `64 passed`.
- The nupkg lists `analyzers/dotnet/cs/timewarp-state-analyzer.dll`, `timewarp-state-analyzer-code-fixes.dll`, and
  `timewarp-state-source-generator.dll`.
- Manual check in an IDE: remove `[JsonSerializable(typeof(WasmCounterState))]` from
  `samples/04-telemetry/wasm/sample-04-wasm/sample-04-wasm-json-context.cs`. `WasmCounterState` then shows TWS0013
  ("…serialized by telemetry snapshots…"), and the light bulb offers "Add [JsonSerializable(typeof(WasmCounterState))] to Sample04WasmJsonContext".

### Review

- **Status:** round 1 findings fixed in the fix pass (M1, M2 both `fixed`). Round 2 is pending.
- **Rounds:** 1, effort 3, roster `general`.
- **Counts (round 1):**
  - bug: 0
  - suggestion: 1 fixed (M1)
  - nit: 1 fixed (M2)
- **M1 (fixed):** the telemetry reason ignores the opt-in `IncludeSnapshots`. The spans-only limit and the `.editorconfig` opt-out are now
  in analyzers.md, the analyzer readme, the release note, and telemetry.md.
- **M2 (fixed):** "Create StateJsonContext" emitted `internal`. It now emits `public` for an effectively public state and `internal`
  otherwise, with a test for each case.
- **Reviewer re-ran:**
  - `./bin/dev build`: 0 errors, 151 pre-existing warnings, no TWS0013 hits.
  - Analyzer tests: 63/63.
  - `ganda repo audit`: exit 0. The 2 Warning-severity failures are older than this branch.
- **Artifacts:**
  - `review/review-framework.md`
  - `review/round-1/general.md`
  - `review/round-1/merged.md` (live ledger)

## Review

- Round 1: Blocked — open M1, M2

## Notes

Steve 2026-11-06 decision; can run alongside Option 1.
