# Round 2 — general
**Date:** 2026-10-29
**Scope reviewed:** the delta since round 1 (base `cac4d431`): commit `b3c309de`. Round 1 reviewed the
uncommitted tree, so this commit holds that product diff plus the fix pass for M1–M4.

## Summary

The fix pass commits the product diff and adds `DevToolsWasmExporter_Should.IgnoreAFrameworkComponentActivity`.
It rewords the package note in `native-trace-sources.cs` and records the WASM inspector check made through the
AppHost proxy `http://localhost:5086`. Risk is low. The product change is sample, docs, and tests. The only library
edits are Design comments in the two DevTools exporters. I found no new issues.

## Re-verification of round 1

- **M1 — fixed.** `b3c309de` holds the 11 modified files and `native-trace-sources.cs`. `git status` shows no
  untracked product files, and `.playwright-cli/` is neither present nor tracked. The checklist is ticked and
  `## Results` is written. I ran `./bin/dev test`: exit 0, with suite counts matching Results (devtools 103 passed,
  including `IgnoreNativeFrameworkActivitySources` and `IgnoreAFrameworkComponentActivity`).
- **M2 — fixed (accepted on recorded evidence).** Results record `/counter-wasm` loaded through the proxy origin
  `http://localhost:5086`, with `POST /ingest/spans` → 200 and a live **wasm** row beside the **srv** row. That origin
  is the CORS origin the AppHost configures. I did not relaunch the AppHost in this headless round.
- **M3 — fixed.** `tests/timewarp-state-devtools-tests/wasm-exporter-tests.cs:49` starts a
  `Microsoft.AspNetCore.Components` activity that carries State tags. It waits for the State activity to reach
  `InspectorLog`, then asserts that the framework entry never arrives. The test can fail: the exporter's
  `ShouldListenTo` (`source/timewarp-state-devtools-wasm/dev-tools-wasm-exporter.cs:90`) is the only filter, and the
  framework activity stops before the State one, so a widened filter would post it in the same or an earlier batch.
- **M4 — fixed.** The Design region now says the package "adds a dependency and no spans".

## Other checks on the delta

- No remaining `AddAspNetCoreInstrumentation` or `OpenTelemetry.Instrumentation.AspNetCore` package reference in
  `samples/`, `source/`, `tests/`, `documentation/`, or `Directory.Packages.props`. Prose mentions remain only to say
  the package is not used.
- The source and operation names match the RC3 shared framework. `Microsoft.AspNetCore.Components.dll` contains
  `Microsoft.AspNetCore.Components.HandleEvent` and the `Event ` display-name prefix. So `samples/overview.md`
  ("under `HandleEvent`") and `samples/04-telemetry/overview.md` (`Event onclick -> …`) describe the same span by
  operation name and by display name.
- Goal restatement vs task: consistent. The task left "remove or keep" open, and the restatement picks remove, with
  the redundancy as the reason. No misread.

## Issues

None.
