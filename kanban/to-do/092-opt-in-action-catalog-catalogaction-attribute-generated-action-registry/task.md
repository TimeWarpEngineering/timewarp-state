# Opt-in action catalog: [CatalogAction] attribute + generated action registry

## Description

Consumers need to enumerate the user-facing actions of an app at runtime — first for a Ctrl-K
command palette (timewarp-architecture task 239, GitHub timewarp-architecture#102), later for agent
tools (WebMCP `registerTool`). Today nothing in TimeWarp.State describes actions: there is no
action catalog, metadata registry, description attribute, or runtime enumeration. `[TrackAction]`
(`source/timewarp-state-plus/features/action-tracking/pipeline/track-action-attribute.cs`) is a
busy-indicator marker and must keep that single meaning. `ActionSetMethodSourceGenerator`
(`source/timewarp-state-source-generator/action-set-method-generator.cs`) already parses each
`*ActionSet.Action`'s first explicit constructor to emit `State.Method(args, ct)`; the catalog
should reuse that parse rather than duplicate it.

Contract agreed in timewarp-architecture task 238 (research; kitchen
`kanban/done/238-evaluate-jev-for-ctrl-k-and-webmcp-action-catalog/research/catalog.json` there):
entries carry `name`, `description`, `execute`, `input_schema`, `visibility`, `permissions`.
Consumers own ranking (deterministic C# shortlist; no LLM ranker) and permission enforcement.

## Requirements

- **Attribute (runtime package, opt-in):** `[CatalogAction(Description = "…")]` on the nested
  `Action` class, with optional `Name` (default `<StateWithoutSuffix>.<ActionSetWithoutSuffix>`,
  e.g. `Credentials.AddPasskey`), `Permissions` (string[] — consumer-defined policy/permission ids,
  opaque to State), and `Visibility` (`Human` | `Agent` | `Both`, default `Human`). `Description` is
  required and must be one plain sentence (analyzer below). Opt-in only: an action without the
  attribute is never cataloged (Fetch*/Clear*/Debug/inbound hub actions stay out by default).
- **Generator:** in the existing source-generator project, emit per cataloged action a descriptor
  (name, description, permissions, visibility, state type, action type, parameter list with name /
  CLR type / optional default — from the same ctor parse as the ActionSet method generator) and one
  per-assembly registry (e.g. `ActionCatalog.All` : `IReadOnlyList<ActionCatalogEntry>`), plus an
  `Execute(IStore, object?[] args, CancellationToken)` (or equivalent typed invoker) that calls the
  same generated `State.Method(...)` — no reflection at runtime, AOT/trim safe.
  Multi-assembly apps: document how a consumer aggregates registries (e.g. assembly-level
  attribute + DI registration helper `AddActionCatalog(typeof(Marker).Assembly)`), no global static
  scanning.
- **Input schema:** expose parameters as data (name, type, required); optionally a JSON-schema
  string for primitives/enums. Complex `Command` parameters: record type name only in v1 and say so.
- **Analyzer (existing analyzer project):** `[CatalogAction]` on a type that is not a nested
  `*ActionSet.Action` → error; missing/empty `Description` → error; duplicate `Name` in one
  assembly → error. Add to `AnalyzerReleases.Unshipped.md`.
- **Tests:** generator snapshot tests (single, multiple, parameters with defaults, name override,
  permissions/visibility), a runtime test in `timewarp-state-tests` or `test-app` that enumerates
  the registry and executes an entry end to end through the store, and analyzer tests for each
  diagnostic.
- **Docs:** README / docs page section "Action catalog" with an example; state explicitly that
  permissions are consumer-enforced and that `[TrackAction]` is unrelated.
- **Release:** ship in the next `12.0.0-beta.N`; timewarp-architecture pins forward (never backward).

## Checklist

- [x] `CatalogActionAttribute` + `ActionCatalogEntry` + visibility enum (runtime package)
- [x] Generator: descriptors + per-assembly registry + typed execute, reusing the ActionSet ctor parse
- [x] Multi-assembly aggregation helper
- [x] Analyzer diagnostics (placement, description, duplicate name)
- [x] Generator, runtime, analyzer tests
- [x] Docs section
- [ ] Released; consumer task notified (timewarp-architecture 239) — version bumped to 12.0.0-beta.6 in this PR; `dev release` + notifying 239 happen after merge

## Notes

- **Do not let `ganda repo audit --fix` rename `tests/test-app/test-app-client/wwwroot/Test.App.Client.lib.module.js`.**
  Blazor loads JS initializers by exact assembly name (`<AssemblyName>.lib.module.js`); the kebab
  audit currently lacks that exception and its auto-fix lowercases the file, which silently breaks
  the test app. If the walk's audit step renames it, revert that rename in the same PR and note it
  in Results (ganda needs an exception — tracked separately).

- Consumer: timewarp-architecture task 239 (Ctrl-K command palette) — its command rows depend on
  this release; navigation rows do not.
- Out of scope: ranking, LLM/Jev integration, WebMCP wiring, changing `[TrackAction]`.
- Cockpit session: https://claude.ai/code/session_01QYpqCSgnvvLRpXrMKxu5ED

## Results

Implemented the opt-in action catalog in TimeWarp.State (version bumped to **12.0.0-beta.6**).

- **Runtime** (`source/timewarp-state/features/action-catalog/`, namespace `TimeWarp.State`):
  `CatalogActionAttribute` (`Description`, `Name`, `Permissions`, `Visibility`), `ActionVisibility`
  (`Human`=1 default, `Agent`=2, `Both`=3, flags), `ActionCatalogEntry` (name, description,
  permissions, visibility, state/action type, parameters, `InputSchema`, `Execute(IStore, object?[]?, ct)`),
  `ActionCatalogParameter` (name, CLR type, required, default text, JSON schema), `ActionCatalogArguments`
  (count/type checks used by generated code), `ActionCatalogProviderAttribute`, `IActionCatalog` /
  `ActionCatalog` (aggregator; cross-assembly duplicate name → `InvalidOperationException`) and
  `services.AddActionCatalog(params Assembly[])`.
- **Generator** (`ActionCatalogSourceGenerator`): `ForAttributeWithMetadataName` on the attribute; emits one
  internal `<AssemblyName>.GeneratedActionCatalog.All` (sorted by name) and an assembly-level
  `GeneratedActionCatalogProviderAttribute` for aggregation. Executors call the generated
  `store.GetState<TState>().Method(args…, externalCancellationToken: ct)` — no reflection; one call per legal
  argument count so trailing optional args use the ctor defaults. The ctor parse was extracted into
  `ActionSetConstructorParser`, now shared by `ActionSetMethodSourceGenerator` (behavior unchanged).
  Input schema: primitives/string/char/Guid/date-time types/enums; complex types → `{"x-clr-type":"…"}` only (v1).
  Nothing is emitted for assemblies without cataloged actions.
- **Analyzer** (`CatalogActionAnalyzer`): TWS0004 placement (error), TWS0005 missing/empty Description (error),
  TWS0006 duplicate Name per assembly (error, compilation end), TWS0007 Description not one plain sentence
  (warning). Added to `AnalyzerReleases.Unshipped.md` and the analyzer readme.
- **Test app**: `EventStream.AddEvent` cataloged; new `CounterState.AddToCountActionSet` (`amount`,
  `multiplier = 1`, permissions `counter.write`, visibility `Both`); `AddActionCatalog` registered in the
  client program and the integration-test host.
- **Tests**: 12 generator tests (full snapshot for a single entry + compile check, multiple/opt-in, defaults /
  enum / nullable / complex params, name override, permissions/visibility, none emitted), 8 analyzer tests,
  8 client integration tests (enumerate registry, DI aggregation, execute end to end through the store,
  optional args, arg count/type errors). Full `scripts/test.cs` green.
- **Docs**: `documentation/topics/action-catalog.md` (+ toc), readme "Action catalog" section, release notes
  `release12.0.0-beta.6.md`. States that permissions are consumer-enforced and `[TrackAction]` is unrelated.
- **Audit**: `ganda repo audit --fix --checks bin-dev,required-gitignore-entries,vscode-window-icon` fixed the
  pre-existing blocking failures (`.gitignore` gains `.local/`; `.vscode/settings.json` gains `peacock.color`;
  `bin/dev` is a local ignored build). Remaining advisory warnings: `kebab-path-names` on
  `Test.App.Client.lib.module.js` (deliberately **not** renamed — Blazor JS initializer name) and
  `memsearch-scaffold` (would set `core.hooksPath`; left for the operator).
- Not done here: the NuGet release itself and notifying timewarp-architecture 239 (post-merge).

### How to validate

**Smoke**

```bash
dotnet fixie timewarp-state-source-generator-tests --tests "ActionCatalogSourceGenerator_*"
dotnet fixie timewarp-state-analyzer-tests --tests "CatalogActionAnalyzer_*"
dotnet build tests/client-integration-tests && dotnet fixie client-integration-tests --tests "ActionCatalog_*"
dotnet run --file ./scripts/test.cs
ganda repo audit
```

**Expect**

- Generator: `12 passed` (includes the persistence tests); analyzer: `8 passed`; client integration: `8 passed`.
- `scripts/test.cs` completes with every suite passing (skips are pre-existing).
- `tests/test-app/test-app-client/generated/timewarp-state-source-generator/**/TimeWarp.State.ActionCatalog.g.cs`
  lists `Counter.AddToCount` and `EventStream.AddEvent` only.
- `ganda repo audit` prints "Repository passes" (advisory warnings only).

## Session

- Created: https://claude.ai/code/session_01QYpqCSgnvvLRpXrMKxu5ED (2026-09-30)
- Implemented under ganda task work (implement oracle), 2026-09-30.
