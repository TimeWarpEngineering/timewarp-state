# Option 1: one State graph, several emitters, and a State manifest

## Description

Source: task 140 `report/analysis.md` (options as code, costs, what each unblocks). Read it first. Option 1. Follows Option 0.

Replace the per-generator scans with one `TimeWarpStateGenerator` in the Mediator style: one equatable graph of states, actions, handlers, catalog entries, persistence and clone facts, built once; emitters for the dispatch methods, catalog, persistence, clone, and a State manifest DevTools and analyzers can read.

## Requirements

- One graph, one builder. Analyzers stay a separate project but read the same fact extractors linked as source. Do not copy Mediator's two-builder split.
- Emitters on the graph for everything the four generators emit today; output identical for existing shapes (golden tests).
- State manifest: states and actions known up front for DevTools, dispatch from DevTools, and for consumer analyzers (architecture's coverage checks).
- Incrementality test on the graph; caching pinned.
- Version bump per the repo's release rule.

## Goal (implementer restatement)

Replace the four `[Generator]` classes (ActionSet method, action catalog, persistence, clone) with one
`TimeWarpStateGenerator`. It builds one equatable `StateGraph` from fact extractors and runs several emitters on it.
Add a State manifest that runtime code (DevTools) and other analyzers can read.

Acceptance criteria, in my words:

1. **One graph, one builder.** Fact extractors turn syntax and symbols into equatable facts: actions (dispatch,
   catalog, manifest), persistent states, and states. One `StateGraphBuilder` merges them into a `StateGraph`. The
   clone planner stays a semantic stage. Its equatable plan feeds the same graph. No second graph builder.
2. **Analyzers share the extractors.** The analyzer project stays separate. It compiles the shared fact files as
   linked source (`<Compile Include=... Link=...>`) and uses them instead of its own copies. That covers the
   catalog placement rule, the default catalog name, assembly visibility, and the `State<T>` inheritance check.
3. **Emitters with golden parity.** Emitters cover the dispatch method, catalog, persistence `Load`, and clone and
   registration. Hint names, generated text and diagnostics stay identical to the old generators for every
   existing shape. Golden files captured from the old generators *before* the rewrite pin this, and the existing
   generator suite still passes.
4. **State manifest.** A new emitter writes `TimeWarp.State.StateManifest.g.cs`. It holds an internal
   `TimeWarpStateManifest` (Version, Json) and `[assembly: TimeWarp.State.StateManifestAttribute(version, json)]`.
   The JSON lists each state (type, persistence, clone kind) and its actions (type, telemetry name, method,
   parameters, catalog name, tracked). A public `StateManifest` reader in `TimeWarp.State` parses it without
   reflection over members. An analyzer can read the attribute from a referenced assembly's metadata, and a test
   proves it.
5. **DevTools consumption.** The DevTools export posts the inspected app's manifests at startup. DevTools ingests
   and keeps them, so the inspector knows states and actions before any span arrives. Dispatch from DevTools gets
   its prerequisites in the manifest (catalog names and parameter schemas). A dispatch transport back into the
   inspected app is a follow-up unless it fits.
6. **Incrementality.** Every step is named. A test pins that an unrelated edit leaves the graph and every output
   `Cached` or `Unchanged` with identical text, and that a relevant edit re-emits.
7. **Release.** `12.0.0-beta.17` is not on NuGet yet (beta.16 is the latest), so the repo rule keeps the version
   and adds this work to the beta.17 release notes. Build 0 errors with no new warnings, tests green,
   `ganda repo audit` clean, committed and pushed on the task branch.
8. **CI against the PR base.** `feature/state-graph` gained TWS0013 (`state-json-serializable-analyzer`) after this
   branch removed `StateSymbolHelpers`. The combined tree must compile. Those two call sites use `StateSymbolFacts`
   (`GetTimeWarpStateType`, `InheritsFromTimeWarpState`), the same methods the other analyzers use. The analyzer
   stays. Proof is a green analyzer build and the TWS0013 tests, then a push that leaves PR #672 open.

Fix pass (round 1 findings), in my words: close all 12 open findings in `review/round-1/merged.md` on this id.
`StateManifest.Parse` must throw only its documented exceptions. The publisher test must really send a request. The
container must own the publisher. Incrementality and TWS0004 coverage gaps get tests. The once-only post and the
absolute-URI rule get documented. Stale comments and names get fixed. The analyzer stops linking `facts\*.cs` by
wildcard, and the golden coverage check names its keys. Done means: build green, every suite green, audit passes,
each finding has a disposition note, pushed.

## Checklist

- [x] Graph and builder
- [x] Emitters with golden parity
- [x] Manifest and DevTools consumption (dispatch from DevTools is a follow-up; the manifest carries what it needs)
- [x] Incrementality test; build 0 errors and no new warnings in changed files (pre-existing NU1510, NU190x, BL0010, TW0007 remain)
- [x] CI merge with `feature/state-graph` compiles (TWS0013 calls `StateSymbolFacts`)

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06)
- 2026-11-06 implementer (claude-opus-5-5, ganda task work implement node): goal restatement. Golden output was
  captured from the four old generators first, in commit 40ea8d00, before any generator change. Then the
  single-graph rewrite, the runtime manifest reader, the analyzer's linked facts, docs and release notes.
  - A helper agent (sonnet) did the DevTools slice: the export publisher, the `/ingest/manifest` endpoint, the
    registry and the Manifest tab. I gave it a written brief and reviewed its diff.
  - Parity on real projects: I extracted the pre-rewrite and post-rewrite trees with `git archive` into /tmp and built
    test-app-client, sample-05-wasm, sample-02-wasm and timewarp-state-devtools with `EmitCompilerGeneratedFiles`.
    All 72 generated files are byte-identical. The only new file is `TimeWarp.State.StateManifest.g.cs`.
  - Audit: `kanban-landed` was an Error because this kitchen differed from origin-home. Fixed with
    `ganda repo audit --fix --checks kanban-landed`. What remains is advisory and pre-existing, the same as task
    141: 098 has no usable worktree, and `kebab-path-names` flags Blazor JS module and MSBuild targets names that
    must match assembly names, plus backlog 110's double hyphen.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 578c5d26-d85d-4c1c-9681-7a6c7a7154bf, max-turns 500, Done — 2026-11-05T20:38:54Z
- 2026-11-06 review oracle (claude-opus-5-5, ganda task work review node, tw-implementation-review, effort 3, roster
  general, turn cap 200). Two read-only general-purpose helper passes ran: the DevTools and runtime manifest slice,
  and the analyzer facts plus test claims. Round 1 is in `review/round-1/`, with 12 open findings (M1 to M12). A
  fix pass is needed.
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 9d35440b-94cd-4f61-880d-4e61e343befa, max-turns 200, artifacts general.md, merged.md — 2026-11-05T20:46:46Z
- 2026-11-06 implementer fix pass (claude-opus-5-5, ganda task work implement-fix-2 node): fixed M1 to M12 from
  round 1. Each disposition note is in `review/round-1/merged.md`. `./bin/dev build` and `./bin/dev test` are green.
  `ganda repo audit` passes with the same 2 pre-existing advisory warnings, so there was nothing new to fix.
- 2026-11-06 review oracle round 2 (claude-opus-5-5, ganda task work review-2 node, tw-implementation-review,
  effort 2, roster general, turn cap 120, no helper agents). It reviewed the fix delta `75b10bdd..138a9298` and ran
  the analyzer, DevTools and generator suites. All are green. Artifacts: `review/round-2/general.md`,
  `review/round-2/merged.md` and `review/disposition.md` (clean).
- 2026-11-06 CI pack failure on PR #672 (DevTools.Export nuspec dependency TimeWarp.State outside the allow-list); allow-list updated to permit TimeWarp.State for DevTools.Export. Local `dev pack` green (packed set verified, 9 nupkgs).
- 2026-11-06 implementer (grok-4.7, ganda task work implement node): PR #672 CI run 42119659172 failed at build. GitHub merges this branch with `feature/state-graph`, which added TWS0013 in `state-json-serializable-analyzer.cs` after this branch deleted `StateSymbolHelpers`. Merged `origin/feature/state-graph` (no conflicts) and pointed the two call sites at `StateSymbolFacts.GetTimeWarpStateType` and `InheritsFromTimeWarpState`. The analyzer stays. PR left open.

## Depends on

- Option 0 task (filed with this one)

## Review

- Round 1: Blocked — open M1, M2, M3, M4, M5, M6, M7, M8, M9, M10, M11, M12
- Fix pass 2026-11-06: M1 to M12 fixed. Awaiting round 2.
- Round 2 (2026-11-06): M1 to M12 verified as fixed. The new nit M13 (a stale Results line) was fixed in the
  kitchen. 0 open. Disposition: clean (`review/disposition.md`).
- Round 2: Done
## Notes

Steve 2026-11-06 decision; order 0 → 1.

## Learnings

- Restatement vs task: the requirement says analyzers "read the same fact extractors linked as source". The
  restatement (criterion 2) and the implementation narrowed that to the per-symbol rule files in `facts/`. The syntax
  extractors in `extractors/` are not linked into the analyzer. The intent of no duplicated rules is met, and the
  `GeneratorSyntaxContext` extractors could not serve an analyzer as they are. The narrowing is a judgment call that
  the restatement did not flag. The review records it as a narrowed reading, not a defect. (why: review oracle round
  1, 2026-11-06)

## Results

### Review

- Round 1 (2026-11-06): effort 3, roster general. Findings: 1 bug, 4 suggestions and 7 nits, all open (M1 to M12).
  See `review/round-1/merged.md`.
- Disposition: the 2026-11-06 fix pass fixed all 12 findings (M1 to M12). The notes are in `merged.md`. Round 2
  re-reviews.
- Behavior changes from the fix pass:
  - `StateManifest.Parse` skips array items that are not JSON objects.
  - The manifest publisher is registered with a factory, so the container disposes it.
  - `InspectorManifest.SetAll` raises `Changed` once per ingest request.
  - The DevTools action `NoteLogChanged` is renamed `NoteDataChanged`. It is internal to the inspector app.
  - The analyzer links the two fact files by name.
- Generator output change (M5): a `[CatalogAction]` action directly inside a class named `ActionSet` (no prefix) no
  longer gets a catalog entry, because the generator now uses the analyzer's placement rule. TWS0004 already made
  that placement a build error, so no clean build changes. The beta.17 release notes record it.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.
- Gates the reviewer ran: DevTools tests 122/122. Helper runs: generator tests 162/162 and analyzer tests 51/51.
- Round 2 (2026-11-06): effort 2, roster general. Scope: the fix delta `75b10bdd..138a9298`.
  - All of M1 to M12 were verified as fixed in the code.
  - One new nit, M13: this Results section still described the analyzer's fact-file links as a `facts\*.cs`
    wildcard. The reviewer fixed it in the kitchen.
  - Gates the reviewer ran: analyzer tests 53/53, DevTools tests 124/124 and generator tests 164/164.
- **Final counts:** bug 1 fixed, suggestion 4 fixed and nit 8 fixed. Nothing is open or wontfix.
- **Disposition: clean.** All 13 findings are fixed.
- Review paths: `review/review-framework.md`, `review/round-2/general.md`, `review/round-2/merged.md` (the final
  ledger) and `review/disposition.md`.

### What changed

- **One generator.** `TimeWarpStateGenerator` (`source/timewarp-state-source-generator/timewarp-state-generator.cs`)
  replaces `ActionSetMethodSourceGenerator`, `ActionCatalogSourceGenerator`, `PersistenceStateSourceGenerator` and
  `StateCloneSourceGenerator`.
  - **Fact extractors** (`extractors/`):
    - `ActionFactExtractor` analyzes each `*ActionSet.Action` once. It produces the dispatch method model or TWSG003,
      the catalog entry and the manifest row.
    - `PersistenceFactExtractor` handles `[PersistentState]`: Load, TWSG001 or TWSG003, plus the persistence method.
    - `StateFactExtractor` finds concrete `IState` types for the manifest.
    - `ActionSetShape` and `ActionSetConstructorParser` moved here unchanged.
  - **Graph** (`graph/`): `StateGraph` is equatable and holds no symbols, syntax or locations. `StateGraphBuilder` is
    the one builder. The clone planner stays a semantic stage (`CloneEmitter.Plan` on `CompilationProvider`), and its
    equatable result feeds the graph.
  - **Emitters** (`emitters/`): `DispatchMethodEmitter`, `CatalogEmitter`, `PersistenceEmitter` and `CloneEmitter`
    produce the old text byte for byte. `ManifestEmitter` is new. Dispatch and Persistence are `SelectMany`
    projections of the graph, so each method or Load is compared and emitted on its own.
  - **Diagnostics:** TWSG001, TWSG002 and TWSG003 descriptors now live together in `generator-diagnostic-info.cs`.
    Release tracking notes name `TimeWarpStateGenerator`.
- **Analyzers share the extractors' rules.** `facts/state-symbol-facts.cs` (`State<T>` and `IState` checks, metadata
  names) and `facts/catalog-action-facts.cs` (catalog placement, visibility, default name, attribute reading) are
  compiled into `timewarp-state-analyzer` as linked source: one `<Compile Include=... Link=...>` per file, named
  explicitly rather than by a `facts\*.cs` wildcard.
  - `CatalogActionAnalyzer` lost its own placement, visibility and default-name copies.
  - `StateSymbolHelpers` is deleted, and the analyzers use `StateSymbolFacts`.
  - TWS0013, which arrived on `feature/state-graph` after that deletion, calls `StateSymbolFacts` too
    (`GetTimeWarpStateType`, `InheritsFromTimeWarpState`). Same methods, same arguments.
  - There is still one graph builder. Analyzers share per-symbol rules, not a graph.
- **State manifest.**
  - `TimeWarp.State.StateManifest.g.cs` holds `[assembly: StateManifestAttribute(Version, Json)]` and an internal
    `TimeWarpStateManifest`. It is emitted only when the assembly declares a state and the referenced TimeWarp.State
    has the attribute.
  - Runtime: `StateManifestAttribute`, `StateManifest` (`FromAssembly`, `Parse`), `StateManifestState`,
    `StateManifestAction` and `StateManifestParameter` (`source/timewarp-state/features/state-manifest/`). Parsing
    uses `JsonDocument`, so it is AOT and trim safe and tolerates unknown fields.
- **DevTools.**
  - Export: `AddTimeWarpStateDevToolsManifest(params Assembly[])` registers `DevToolsManifestPublisher`, a hosted
    service that posts once at start. It is a no-op without `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` and never throws.
    `ManifestIngestUri` is new. The export package now references `TimeWarp.State`.
  - DevTools: `POST /ingest/manifest` (415, 413 and 400 like spans, the same CORS rule, capped counts) feeds
    `InspectorManifest`, a singleton that keeps the newest manifest per assembly and raises `Changed`. A new
    **Manifest** tab (`ManifestPanel.razor`) lists states and actions.
  - Dispatch from DevTools is not built. It needs a channel back into the inspected app, which is a follow-up. The
    manifest already carries the method, parameters and catalog name it would use.
- **Tests.**
  - Generator suite, 150 to 162:
    - `golden-parity-tests.cs` has 5 cases plus a coverage check. The `golden/*.verified.txt` files were written by
      the old generators.
    - `generator-incrementality-tests.cs` was rewritten with 5 cases for the graph.
    - `state-manifest-tests.cs` has 10 cases: exact JSON, no-emit cases, Parse, a newer version with unknown fields,
      a loaded-assembly round trip, and the analyzer reading metadata.
    - Existing tests now point at the one generator, and drivers filter by hint name.
  - DevTools suite: +14 (`manifest-ingest-tests.cs`).
- **Docs:** a new `documentation/topics/state-manifest.md` with its toc entry, a DevTools Manifest tab section,
  updated generator names in `analyzers.md`, `cloning.md`, the analyzer readme and `claude.md`, both DevTools
  readmes, and a task 142 section in `release12.0.0-beta.17.md`.
- **Version:** `12.0.0-beta.17` is not on NuGet (beta.16 is the latest), so the repo rule keeps beta.17 and adds
  these notes to it.

### Measured incrementality (Roslyn 4.14.0)

- **Unrelated file added:** `State.Actions`, `State.Persistence` and `State.States` are `Cached` or `Unchanged`.
  `State.Assembly` and `State.ClonePlan` are `Unchanged`. `State.Graph` is `Cached`, every `State.Emit.*` step is
  `Cached`, and every output is `Cached` with identical text.
- **Edit a method above the action in the same file:** the same as above, with no output re-added.
- **Change an action parameter:** Actions, Graph, Emit.Dispatch, Emit.Catalog and Emit.Manifest are `Modified`.
  Emit.Persistence stays `Cached` or `Unchanged`.
- **Add a state property:** ClonePlan, Graph and Emit.Clone are `Modified`. Dispatch, persistence, catalog and
  manifest stay `Cached` or `Unchanged`. Exactly one output is `Modified`.

### Gates run

- `./bin/dev build`: success.
- `./bin/dev test` after the fix pass: all suites green. Analyzer 53, generator 164, state 146 (1 skipped), plus 40
  (1 skipped), telemetry 21, devtools 124, devtools isolation 4, client integration 64 (1 skipped), architecture 7
  (1 skipped). Before the fix pass: analyzer 51, generator 162, devtools 122.
- Solution build: 0 errors, and no warnings from any file this task changed.
- `ganda repo audit`: passes, with 2 pre-existing advisory warnings.
- Real-project parity: all 72 pre-existing generated files are byte-identical across 4 projects.
- 2026-11-06 CI compile fix (merge of `feature/state-graph` plus the TWS0013 call-site change): `./bin/dev build` success, 0 errors, and no warning on `state-json-serializable-analyzer.cs`. `./bin/dev test` green: analyzer 66, generator 164, state 146 (1 skipped), plus 40 (1 skipped), telemetry 21, devtools 124, devtools isolation 4, client integration 64 (1 skipped), architecture 7 (1 skipped). `ganda repo audit` still passes with the same 2 advisory warnings (task 098 has no usable worktree; kebab names that must match assembly names, plus backlog 110).

### How to validate

Smoke:

```bash
dotnet build tests/timewarp-state-source-generator-tests -v q -nologo
dotnet fixie timewarp-state-source-generator-tests --tests "*GoldenParity*"
dotnet fixie timewarp-state-source-generator-tests --tests "*GeneratorIncrementality*"
dotnet fixie timewarp-state-source-generator-tests --tests "*StateManifest_*"
# touch forces CoreCompile; --no-incremental trips an unrelated static-web-asset check in the Blazor project
touch tests/test-app/test-app-client/features/counter/actions/counter-state.increment-counter.cs
dotnet build tests/test-app/test-app-client -p:EmitCompilerGeneratedFiles=true \
  -p:CompilerGeneratedFilesOutputPath=/tmp/tw142-gen
ls /tmp/tw142-gen/timewarp-state-source-generator/TimeWarp.State.SourceGenerator.TimeWarpStateGenerator/
dotnet fixie timewarp-state-source-generator-tests --tests "*Should_Read_Manifest*"
dotnet fixie timewarp-state-analyzer-tests --tests "*Should_Report_TWS0004*"
dotnet build source/timewarp-state-analyzer/timewarp-state-analyzer.csproj -v q -nologo
dotnet fixie timewarp-state-analyzer-tests --tests "*TWS0013*"
dotnet fixie timewarp-state-devtools-tests --tests "*Manifest*"
./bin/dev test
```

Expect:

- GoldenParity: 6 passed. Generated text and diagnostics match the files the old generators wrote.
- GeneratorIncrementality: 5 passed.
- StateManifest_: 10 passed.
- The test-app folder lists `*_Method.g.cs`, `TimeWarp.State.ActionCatalog.g.cs`, `TimeWarpStateClones.g.cs` and
  `TimeWarp.State.StateManifest.g.cs`. Its `Json` constant lists `CounterState` with `IncrementCountActionSet.Action`.
- Should_Read_Manifest includes `Given_Non_Object_Array_Items_Skips_Them` and
  `Given_Non_Array_Collections_Reads_Them_As_Empty`, and both pass.
- Should_Report_TWS0004 includes `Given_Bare_ActionSet_Container` and `Given_Struct_Action`, and both pass.
- The analyzer project builds with 0 errors. TWS0013: 13 passed (`Should_Trigger` 4, `Should_Not_Trigger` 4, `Should_Fix` 5).
- The DevTools manifest tests include `StoreABatchAndRaiseChangedOnce` and
  `CollectTheManifestOfAnAssemblyWithStates`, and both pass.
- `./bin/dev test` ends with "Tests completed successfully!".

### Follow-ups

- Dispatch from DevTools: a channel from the inspector into the inspected app. The catalog executors and the
  manifest already supply the call.
- Wiring `AddTimeWarpStateDevToolsManifest` into the samples or test-app AppHost wiring is left to the consumer. It
  is opt-in, like `AddActionCatalog`.

