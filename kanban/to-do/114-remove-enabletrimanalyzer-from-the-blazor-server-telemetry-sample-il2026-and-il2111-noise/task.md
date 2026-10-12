# Remove EnableTrimAnalyzer from the Blazor Server telemetry sample (IL2026 and IL2111 noise)

## Description

Running the telemetry sample AppHost prints trim warnings from `sample-04-server`:

- `IL2026` at `program.cs(22,5)`: `AddRazorComponents(...)` and `AddInteractiveServerComponents(...)`
  carry `RequiresUnreferencedCodeAttribute` — "Razor Components does not currently support trimming or
  native AOT" / "Server-side Blazor does not currently support native AOT".
- `IL2111` in generated `Routes_razor.g.cs(88,13)`: `Router.NotFoundPage.set` accessed via reflection.

Root cause: `samples/04-telemetry/server/sample-04-server/sample-04-server.csproj` line 7 sets
`<EnableTrimAnalyzer>true</EnableTrimAnalyzer>`. That runs the ILLink Roslyn analyzer over a Blazor
Server app. Every warning it reports here comes from framework APIs that are declared unsupported for
trimming, so nothing in the sample can satisfy the analyzer. The flag was introduced in task 058's
telemetry commit (`3a59c4eb`, 2026-09-22) with no recorded reason; no other sample, the test app, or the
DevTools app sets it.

Trim safety is already enforced where it belongs: the packages (`TimeWarp.State`, `.Blazor`, `.Plus`,
`.Telemetry`) set `IsAotCompatible` with the IL2xxx/IL3xxx set as errors, and task 109 M2 added a
`PublishTrimmed` smoke of `samples/05-persistence` (WASM, where trimming is real) to `dev verify-samples`.
A Blazor Server consumer cannot be trim-clean, so analysing one proves nothing about the packages.

Filed by Claude (cockpit) at Steven's request, 2026-10-12.

## Requirements

- Remove `EnableTrimAnalyzer` from `sample-04-server.csproj`. Do not suppress the warnings with
  `NoWarn`; the analyzer should simply not run on a Blazor Server host.
- Add a one-line comment in the csproj (or the sample's Purpose region) saying server-side Blazor is not
  trimmable and package trim checks live in `IsAotCompatible` + the WASM `PublishTrimmed` smoke.
- Check the DevTools app (`source/timewarp-state-devtools`, also Blazor Server) builds without trim
  warnings; it currently sets no flag, so this should already be true. Confirm, do not change.
- `dotnet build samples/04-telemetry/apphost/sample-04-apphost.csproj` and the sample server build with
  zero IL2xxx/IL3xxx warnings. `dev verify-samples` still green.
- If task 058's intent was a consumer-side trim smoke for `TimeWarp.State.Telemetry`, record in
  `### Review disposition

- Rounds: 1. Roster: general (effort 1, Claude review oracle).
- Final counts: bug 0, suggestion 0, nit 0 (0 open, 0 fixed, 0 wontfix).
- Disposition: **clean**. No findings raised.
- Artifacts: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Notes` that the right place is a WASM sample (05) or a dedicated trimmed test app, and leave it
  as a note — do not add it here.

## Checklist

- [x] Remove `EnableTrimAnalyzer` from `sample-04-server.csproj`; comment added
- [x] Sample 04 server and AppHost build with no IL2026 / IL2111 warnings
- [x] DevTools app confirmed warning-free (no change)
- [x] `dev verify-samples` green
- [x] `ganda repo audit` clean; tests green

## Session

- Created: cockpit (2026-10-12)
- Implemented: Grok (2026-10-12)
- Reviewed: Claude review oracle, effort 1, general (2026-10-12) — clean

## Results

`sample-04-server.csproj` no longer sets `EnableTrimAnalyzer`. A comment in that project states that server-side Blazor is not trimmable and that package trim checks live in `IsAotCompatible` and the WASM `PublishTrimmed` smoke. `NoWarn` was not used to hide IL2026 or IL2111.

Rebuilds of the sample server and the AppHost succeeded with 0 warnings and no IL2xxx/IL3xxx diagnostics. `source/timewarp-state-devtools` was not edited. Its build emitted no IL2xxx/IL3xxx warnings. Referenced libraries still emit pre-existing NU1510, TW0007, and BL0010/BL0016 warnings; those are outside this change.

`./bin/dev test` passed: analyzer 44, source generator 80, state 115 (1 skipped), plus 40 (1 skipped), telemetry 19, devtools 45, devtools isolation 4, client integration 65 (1 skipped), architecture 7 (1 skipped). 419 passed, 4 skipped, 0 failed.

`./bin/dev verify-samples` verified 12 sample projects and the trimmed publish of `samples/05-persistence`. No IL2xxx/IL3xxx warnings. One pre-existing ASPDEPR011 remains on sample 00 (`UseWebAssemblyDebugging`).

`ganda repo audit` passed (29 pass, 1 skip, 1 non-blocking kebab-path advisory on the backlog 110 folder and existing `*.lib.module.*` names).

### How to validate

Smoke: `dotnet build samples/04-telemetry/server/sample-04-server/sample-04-server.csproj` and `dotnet build samples/04-telemetry/apphost/sample-04-apphost.csproj`. Then `./bin/dev pack` and `./bin/dev verify-samples`.

Expect: both sample builds succeed with 0 warnings and the logs contain no IL2026 or IL2111. `verify-samples` prints `Samples verified successfully!`, including the trimmed publish of sample 05. If restore still resolves `TimeWarp.State.Plus` 12.0.0-beta.11 from an older global-packages folder, delete that version folder and restore again so the local feed package is the one that contains `AddTimeWarpStatePersistence`.

## Notes

- Related: 058 (introduced the flag), 109 M2 (package `IsAotCompatible` and the WASM `PublishTrimmed`
  smoke), 103 / 111 / 113 (DevTools and sample 04 wiring).
- Repro: `dotnet run --project samples/04-telemetry/apphost/sample-04-apphost.csproj` on master
  `be10a2ed`; the warnings appear while the AppHost builds the sample server.
- Task 058 required "Trim-warn the sample" as a consumer-side trim smoke for `TimeWarp.State.Telemetry`.
  Its results already recorded that those warnings are Blazor Server framework APIs
  (`AddRazorComponents` / `Router`), not the package. The right place for that consumer check is the
  WASM sample (05) `PublishTrimmed` gate or a dedicated trimmed test app. Not added on this host.
