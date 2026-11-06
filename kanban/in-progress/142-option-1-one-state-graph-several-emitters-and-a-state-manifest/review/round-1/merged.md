# Round 1 — merged findings
**Date:** 2026-11-06
**Sources:** general (with re-verified helper passes: DevTools/runtime manifest, analyzer facts/tests)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 4 | 0 |
| nit | 0 | 7 | 0 |

## Issues

Full descriptions and evidence are in `general.md` (Issue N maps to MN).

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state/features/state-manifest/state-manifest.cs:125-129
- Description: `StateManifest.Parse` throws `InvalidOperationException` on array items that are not objects. The
  public contract documents only `JsonException` and `FormatException`.
- Suggestion: skip non-object items in `Array()`, or guard `String` and `Boolean` on the element kind. Add tests.
- Source: general (DevTools helper)
- Disposition notes: Fixed. `Array()` now skips items that are not JSON objects, so `String`/`Boolean` only read object properties. Tests `Should_Read_Manifest.Given_Non_Object_Array_Items_Skips_Them` and `Given_Non_Array_Collections_Reads_Them_As_Empty`.

### M2 — Severity: suggestion — Status: fixed
- File: tests/timewarp-state-devtools-tests/manifest-ingest-tests.cs:177-182
- Description: `NotThrowWhenDevToolsIsDown` uses an assembly with no manifest, so no HTTP call happens.
- Suggestion: use an assembly that has a manifest, such as `typeof(InspectorManifest).Assembly`.
- Source: general (DevTools helper)
- Disposition notes: Fixed. `NotThrowWhenDevToolsIsDown` now uses `typeof(InspectorManifest).Assembly` (it declares `InspectorState`, so a request is really sent). New `CollectTheManifestOfAnAssemblyWithStates` pins that the assembly has a manifest.

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-devtools-export/timewarp-state-dev-tools-export.cs:114-115
- Description: the hosted publisher is registered as a pre-built instance, so the container never disposes it or its
  `HttpClient`.
- Suggestion: register it with a factory.
- Source: general (DevTools helper)
- Disposition notes: Fixed. `AddTimeWarpStateDevToolsManifest` registers the publisher with a factory, so the container owns and disposes it and its `HttpClient`.

### M4 — Severity: suggestion — Status: fixed
- File: tests/timewarp-state-source-generator-tests/generator-incrementality-tests.cs:141-161
- Description: the action-edit case does not assert that `EmitClone` stays cached.
- Suggestion: add the `EmitClone` cached-or-unchanged assertion.
- Source: general (analyzer/tests helper)
- Disposition notes: Fixed. `Given_Action_Parameter_Changed` asserts `EmitClone` is cached or unchanged.

### M5 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-source-generator/facts/catalog-action-facts.cs:45; tests/timewarp-state-analyzer-tests/catalog-action-analyzer-tests.cs
- Description: the generator now skips a bare `ActionSet` container (an output change no test covers). There is no
  TWS0004 test for a bare `ActionSet` or a struct `Action`.
- Suggestion: add the two TWS0004 tests and note the generator change in Results or the release notes.
- Source: general (analyzer/tests helper)
- Disposition notes: Fixed. TWS0004 tests `Given_Bare_ActionSet_Container` and `Given_Struct_Action` (CS0592 plus TWS0004). The bare `ActionSet` generator change is in the beta.17 release notes and in Results.

### M6 — Severity: nit — Status: fixed
- File: source/timewarp-state-devtools-export/dev-tools-manifest-publisher.cs:95-98
- Description: the manifest is posted once. After a DevTools restart, the Manifest tab stays empty until the app
  restarts.
- Suggestion: document this in the readme, or re-post.
- Source: general
- Disposition notes: Fixed (documented). The export readme, DevTools readme, XML doc, publisher Design region and release notes say the post is once and not retried. Re-posting stays out of scope.

### M7 — Severity: nit — Status: fixed
- File: source/timewarp-state-devtools-export/timewarp-state-dev-tools-export.cs:96-111
- Description: an endpoint that is not an absolute URI is silently ignored, and the doc does not say so.
- Suggestion: extend the XML doc.
- Source: general
- Disposition notes: Fixed. The XML doc, export readme and release notes say a value that is not an absolute URI is ignored without an error.

### M8 — Severity: nit — Status: fixed
- File: source/timewarp-state-devtools/dev-tools-ingest-cors.cs:2, 20, 78-80
- Description: the comments still name only the span route.
- Suggestion: mention `/ingest/manifest`.
- Source: general
- Disposition notes: Fixed. The Purpose, Design, class summary and `Use` doc name both `/ingest/spans` and `/ingest/manifest`.

### M9 — Severity: nit — Status: fixed
- File: source/timewarp-state-devtools/components/pages/Inspector.razor:19, 34
- Description: manifest changes reuse the `OnLogChanged` and `NoteLogChanged` names. `Changed` fires once per
  manifest.
- Suggestion: use a neutral name. Optionally raise `Changed` once per request.
- Source: general
- Disposition notes: Fixed. Handlers renamed `OnInspectorDataChanged`, `RefreshSafeAsync`, `RefreshAsync`. Action renamed `NoteLogChanged` to `NoteDataChanged` (file `inspector-state.note-data-changed.cs`). New `InspectorManifest.SetAll` stores a request under one lock and raises `Changed` once; the endpoint uses it. Test `StoreABatchAndRaiseChangedOnce`.

### M10 — Severity: nit — Status: fixed
- File: source/timewarp-state-source-generator/facts/catalog-action-facts.cs:10
- Description: the Design note says "explicit usings", but the file has none.
- Suggestion: align the wording with `state-symbol-facts.cs`.
- Source: general
- Disposition notes: Fixed. The Design wording now matches `state-symbol-facts.cs` (shared global usings, no generator-only types, internal).

### M11 — Severity: nit — Status: fixed
- File: source/timewarp-state-analyzer/timewarp-state-analyzer.csproj:20
- Description: the `facts\*.cs` wildcard would link any future generator-only file into the analyzer.
- Suggestion: list the files explicitly, or write down the folder contract.
- Source: general
- Disposition notes: Fixed. The analyzer csproj lists `catalog-action-facts.cs` and `state-symbol-facts.cs` explicitly, with a comment on why.

### M12 — Severity: nit — Status: fixed
- File: tests/timewarp-state-source-generator-tests/golden-parity-tests.cs:95-96
- Description: the coverage check only counts cases.
- Suggestion: iterate over the keys, or leave it with a comment.
- Source: general
- Disposition notes: Fixed. `Given_Every_Case_Has_A_Test` asserts the exact sorted key sets of `Cases` and `BareCases`.

## Duplicates / conflicts

- None. The helper passes covered disjoint areas (DevTools and runtime manifest; analyzer facts and tests). The
  generator core (graph, builder, extractors, emitters) was reviewed directly, and nothing in it was raised.
- Checked and not raised:
  - Golden parity provenance.
  - Incrementality step wiring.
  - Clone-plan Diagnostic equality (unchanged from task 141).
  - Manifest JSON escaping and its field names against `Parse`.
  - CORS, body-size and status-code parity with span ingest.
  - `InspectorManifest` locking.
  - Razor event unsubscription.
  - The version rule (beta.16 is the latest on NuGet).
