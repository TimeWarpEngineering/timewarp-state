# Round 3 — general
**Date:** 2026-10-11
**Scope reviewed:** fix delta `fdb92fda..03d4d931` (PR #635 NU1102 restore fix). Carries round 2's M1–M11 forward.

## Summary

The isolation tests used to project-reference `samples/04-telemetry/server/sample-04-server`, whose PackageReferences point at an unpublished `TimeWarp.State` 12.0.0-beta.11. CI's pre-pack build therefore failed with NU1102. The fix adds a fixture, `tests/timewarp-state-devtools-inspected-app`. It project-references `TimeWarp.State`, the analyzer and the clone generator from source, plus the exporter. `TimeWarp.Mediator.Generators` is a package. The fixture is in `timewarp-state.slnx` under `/Tests/`, so the generated `artifacts/timewarp-state.build.slnf` picks it up (only `samples/` is skipped). The test project drops its `TimeWarp.State` PackageReference, and the test names follow the new namespace. The four assertions are unchanged in substance: the fixture references Export and not DevTools, the clone registry holds `CounterState` and no `InspectorState`, the sender has an `IncrementCount` dispatch method and nothing named Inspector, and Export has no inspector states. Low risk, test-only.

Verified locally: `dotnet build tests/timewarp-state-devtools-isolation-tests` gave 0 errors. `dotnet fixie timewarp-state-devtools-isolation-tests` gave 4 passed. `scripts/test.cs` already builds and runs that suite by path. The analyzer and source-generator ProjectReferences match `tests/timewarp-state-tests`. `Program.ExporterAssembly` keeps the Export reference in metadata, which `NotReferenceTheDevToolsApp` asserts.

Prior findings M1–M11 stay `fixed`. The delta does not touch DevTools, Export or Hosting product code.

## Issues

None.
