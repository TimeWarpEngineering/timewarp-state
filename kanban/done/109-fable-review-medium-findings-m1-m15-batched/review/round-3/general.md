# Round 3 — general
**Date:** 2026-10-11
**Scope reviewed:** fix delta after the round-2 disposition, `2d150884..d2e2e215` (commit `1c8b959d`, PR #634 e2e fix): `component-trim-roots.cs` in Blazor and Plus, the `RootICloneableStates` emit in `state-clone-planner.cs`, the `EnsureStates` suppression text, `PersistenceTestPage.razor`, `ServerSidePersistenceTestPage.razor`, and the generator test.

## Summary

The delta makes a trimmed WebAssembly client of the test app go interactive. It roots the public components in Blazor (`ReduxDevTools`, `RenderModeDisplay`, `TimeWarpJavaScriptInterop`) and Plus (`TwPageTitle`, `TwBreadcrumb`, `TimeWarpPageRenderNotifier`). Those are all the public components in the two packages. It also emits a module initializer that keeps public constructors and methods on ICloneable states, and the two persistence pages wait for storage load before they render the counters. Risk is low. The generator skips open generic definitions and types it cannot name, and the updated test checks that the output compiles with the `RootStateForTrim<global::HandState>()` call.

## Issues

None. Points I checked and did not raise:

- `RootICloneableStates` is declared on the `file static class TimeWarpStateClones`. The generator test compiles the output with no errors, and the published probe in Results reached WebAssembly.
- The pages set `StorageReady` only after both `WaitForInitializationAsync` calls finish. During prerender they return early, and the markup stays on the empty placeholders. The e2e assertions read the post-load values.
- Blazor uses `[ModuleInitializer]` through a global using, and Plus spells out the full name. Both compile. The difference is style only.
