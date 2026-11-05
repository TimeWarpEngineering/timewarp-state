# Round 2 — merged findings
**Date:** 2026-11-06
**Sources:** general

## Counts

The table covers the carried M1 to M12 and the new M13.

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 4 | 0 |
| nit | 0 | 8 | 0 |

## Resolved prior (round 1)

Each fix below was re-verified against `75b10bdd..138a9298`. The reviewer also ran the analyzer suite (53/53), the
DevTools suite (124/124) and the generator suite (164/164).

| ID | Severity | Status | Verified by |
|----|----------|--------|-------------|
| M1 | bug | fixed | `state-manifest.cs` `Array()` keeps only object items. `Given_Non_Object_Array_Items_Skips_Them` and `Given_Non_Array_Collections_Reads_Them_As_Empty` pass. |
| M2 | suggestion | fixed | `NotThrowWhenDevToolsIsDown` uses `typeof(InspectorManifest).Assembly`. `CollectTheManifestOfAnAssemblyWithStates` pins that it has a manifest. |
| M3 | suggestion | fixed | `AddSingleton<IHostedService>(_ => new DevToolsManifestPublisher(...))`, so the container owns and disposes the publisher. |
| M4 | suggestion | fixed | `Given_Action_Parameter_Changed` asserts `EmitClone` is cached or unchanged. |
| M5 | suggestion | fixed | The TWS0004 tests `Given_Bare_ActionSet_Container` and `Given_Struct_Action` pass. The beta.17 release notes record the generator change. |
| M6 | nit | fixed | The XML doc, both readmes, the Design region and the release notes say the post is once, with no retry. |
| M7 | nit | fixed | The XML doc, the export readme and the release notes say a value that is not an absolute URI is ignored. |
| M8 | nit | fixed | The CORS Purpose, Design, summary and `Use` doc name both routes. |
| M9 | nit | fixed | The rename to `OnInspectorDataChanged` and `NoteDataChanged` is complete (a grep finds no stale names). `SetAll` raises `Changed` once, and `StoreABatchAndRaiseChangedOnce` pins it. |
| M10 | nit | fixed | The Design wording matches `state-symbol-facts.cs`. |
| M11 | nit | fixed | The analyzer csproj lists both fact files by name. |
| M12 | nit | fixed | `Given_Every_Case_Has_A_Test` asserts the exact sorted key sets. |

## Issues

### M13 — Severity: nit — Status: fixed
- File: kanban/in-progress/142-option-1-one-state-graph-several-emitters-and-a-state-manifest/task.md (Results)
- Description: Results still say the analyzer links `facts\*.cs`, which the M11 fix replaced.
- Suggestion: name the two linked files.
- Source: general
- Disposition notes: The reviewer fixed this in `## Results`, a kitchen-only edit. No product change.

## Duplicates / conflicts

- None.
