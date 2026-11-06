# Round 1 — general
**Date:** 2026-11-06
**Scope reviewed:** the whole branch diff vs master (see review-framework.md)

## Summary

The four generators are replaced by one `TimeWarpStateGenerator`. Three syntax-provider fact extractors and two
compilation-level facts feed one `StateGraphBuilder`. Six emit projections hang off the graph. The State manifest is
new: generator emitter, runtime reader, DevTools export publisher, ingest endpoint and Manifest tab.

The core rewrite is sound:
- The golden files come from 40ea8d00 (the old generators) and have not changed since. A missing golden fails the test.
- The incrementality tests fail on unknown step names or empty step sets, so they cannot pass vacuously.
- Analyzer behavior matches master; the analyzer suite is unchanged and passes 51/51.
- Version beta.17 is right: NuGet's flat container lists beta.16 as the latest.

The defects are at the edges:
- The new public `StateManifest.Parse` can throw an undocumented exception.
- One DevTools test is vacuous.
- The hosted publisher is never disposed.
- A few coverage gaps and stale comments.

Verified locally:
- DevTools tests: 122/122 pass.
- Generator tests: 162/162 pass (helper run).
- Analyzer tests: 51/51 pass (helper run).

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-state/features/state-manifest/state-manifest.cs:125-129
- Description: `Array()` checks that the parent is an object. It returns every array item regardless of kind, though.
  `String()` and `Boolean()` then call `JsonElement.TryGetProperty` on an item that may not be an object. That throws
  `InvalidOperationException`. Example inputs: `{"version":1,"states":[1]}` and `{"version":1,"states":[{"actions":[null]}]}`.
  - The public `Parse` documents only `JsonException` and `FormatException`. `FromAssembly` inherits the same gap.
  - The DevTools ingest is safe only because its own `TryParse` also catches `InvalidOperationException`
    (`source/timewarp-state-devtools/manifest-ingest-endpoint.cs:122`). Any other caller, such as a startup check or a
    tool, gets an exception the contract does not mention.
- Suggestion: skip non-object items in `Array()` (`ValueKind == JsonValueKind.Object`), or guard `String` and `Boolean`
  on the element kind. Add Parse tests for a non-object state, action and parameter item.
- Status: open

### Issue 2 — Severity: suggestion
- File: tests/timewarp-state-devtools-tests/manifest-ingest-tests.cs:177-182
- Description: `NotThrowWhenDevToolsIsDown` publishes `typeof(string).Assembly`, which has no manifest.
  - `PublishAsync` returns false before any HTTP call (`dev-tools-manifest-publisher.cs:71-74`), so the "DevTools is
    down" path (a connection refused on port 1) never runs.
  - `ContributeNothingFromAnAssemblyWithoutAManifest` (line 174) relies on that same assembly carrying no manifest.
- Suggestion: publish an assembly that has a generated manifest, such as `typeof(InspectorManifest).Assembly`, so the
  POST is attempted and fails.
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-state-devtools-export/timewarp-state-dev-tools-export.cs:114-115
- Description: `services.AddSingleton<IHostedService>(publisher)` registers an instance that was already built. The
  container does not dispose instances it did not create, so `DevToolsManifestPublisher.Dispose` and its `HttpClient`
  are never called at shutdown.
- Suggestion: register a factory, `services.AddSingleton<IHostedService>(_ => new DevToolsManifestPublisher(uri, assemblies))`.
- Status: open

### Issue 4 — Severity: suggestion
- File: tests/timewarp-state-source-generator-tests/generator-incrementality-tests.cs:141-161
- Description: `Given_Action_Parameter_Changed` lives in the "re-emit only what changed" group, but it checks only that
  `EmitPersistence` stays cached. `EmitClone` is not asserted, so a regression that re-emits the clone file on an
  action edit would pass.
- Suggestion: add `TrackedReasons(result, TrackingNames.EmitClone).ShouldAllBe(IsCachedOrUnchanged)`.
- Status: open

### Issue 5 — Severity: suggestion
- File: source/timewarp-state-source-generator/facts/catalog-action-facts.cs:45; tests/timewarp-state-analyzer-tests/catalog-action-analyzer-tests.cs
- Description: the shared placement rule rejects a container named exactly `ActionSet`.
  - The old catalog generator used only `EndsWith("ActionSet")`
    (`master:source/timewarp-state-source-generator/action-catalog-source-generator.cs:85`), so the generator's output
    changed for that shape. A bare `ActionSet` now gets no catalog entry, where before it got the name `"<State>."`.
  - That shape already hit TWS0004 (Error), so no compiling project is affected.
  - Neither the golden files nor any test cover the change. No TWS0004 test covers the class-only rule that the
    analyzer still enforces itself (`catalog-action-analyzer.cs:146`, for example a `record struct Action`), either.
- Suggestion: add TWS0004 cases for a bare `ActionSet` container and a struct `Action`. Note the generator-side change
  in the task Results or the beta.17 release notes.
- Status: open

### Issue 6 — Severity: nit
- File: source/timewarp-state-devtools-export/dev-tools-manifest-publisher.cs:95-98
- Description: the manifest is posted once at host start. If DevTools restarts while the app keeps running, the
  in-memory `InspectorManifest` is empty until the app restarts. The readme does not say so.
- Suggestion: document the limitation in the export readme, or add a re-post later.
- Status: open

### Issue 7 — Severity: nit
- File: source/timewarp-state-devtools-export/timewarp-state-dev-tools-export.cs:96-111
- Description: a `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` that is set but not an absolute URI is silently ignored here.
  `AddTimeWarpStateDevToolsExporter` throws for the same value. The XML doc mentions only "unset or blank".
- Suggestion: add "or not an absolute URI" to the doc.
- Status: open

### Issue 8 — Severity: nit
- File: source/timewarp-state-devtools/dev-tools-ingest-cors.cs:2, 20, 78-80
- Description: the Purpose region, the class summary and the `Use()` summary still mention only the span route. The
  policy now covers `/ingest/manifest` too.
- Suggestion: mention the manifest route in all three.
- Status: open

### Issue 9 — Severity: nit
- File: source/timewarp-state-devtools/components/pages/Inspector.razor:19, 34; features/inspector/inspector-state.note-log-changed.cs:2
- Description: manifest changes reuse `OnLogChanged` and `NoteLogChanged`, whose names and comments say "InspectorLog
  changed". Also, `InspectorManifest.Set` raises `Changed` once per manifest, so one 32-entry post queues 32 dispatches
  per circuit.
- Suggestion: rename to something neutral, for example `OnInspectorDataChanged`. Optionally raise `Changed` once per
  ingest request.
- Status: open

### Issue 10 — Severity: nit
- File: source/timewarp-state-source-generator/facts/catalog-action-facts.cs:10
- Description: the Design note says the file uses "explicit usings", but it has none. It relies on ImplicitUsings plus
  each project's global usings, as `state-symbol-facts.cs` correctly states.
- Suggestion: align the wording.
- Status: open

### Issue 11 — Severity: nit
- File: source/timewarp-state-analyzer/timewarp-state-analyzer.csproj:20
- Description: `facts\*.cs` is a wildcard. Any file added later to the generator's `facts/` folder compiles into the
  analyzer, including one that uses generator-only types such as `EquatableArray` or the models.
- Suggestion: list the two files explicitly, or write the folder's contract (Roslyn and BCL only) in the csproj
  comment or a Design region.
- Status: open

### Issue 12 — Severity: nit
- File: tests/timewarp-state-source-generator-tests/golden-parity-tests.cs:95-96
- Description: `Given_Every_Case_Has_A_Test` checks only that there are 5 cases. It does not check that each key maps
  to a test.
- Suggestion: iterate over the keys, or leave it as it is with a comment.
- Status: open
