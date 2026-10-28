# Add native .NET 11 OpenTelemetry tracing to State samples and devtools

## Goal (implementer restatement)

.NET 11 already emits HTTP, Blazor component, Blazor Server circuit, and SignalR spans on framework
ActivitySources, with the OpenTelemetry semantic tags on the HTTP span. The telemetry sample should
subscribe to those sources and stop calling `AddAspNetCoreInstrumentation()`, because that package
listens to the same sources and is redundant on this runtime. One click must produce a single
component event span, with the TimeWarp.State action span as its child. DevTools keeps ingesting
only `TimeWarp.State` spans, on the server and in WebAssembly, so the new framework spans do not
show up as inspector rows. The sample overview names the sources and the subscription.

Done when the server sample no longer references `OpenTelemetry.Instrumentation.AspNetCore`, the
overview describes the native sources, DevTools tests reject those source names, and the AppHost
trace for one counter click has one HandleEvent span with the State action nested under it.

## Description

.NET 11 ASP.NET Core and Blazor (including Blazor WebAssembly) emit OpenTelemetry traces with the
standard semantic conventions out of the box. Use that built-in tracing in the TimeWarp.State
samples and devtools, and drop the manual ASP.NET Core / Blazor WASM instrumentation where the
framework now covers it.

Source: item **E** of the 2026-10-20 eval of Daniel Roth's .NET 11 ASP.NET Core and Blazor video
([eval](https://github.com/TimeWarpEngineering/timewarp-intel/blob/master/evaluations/2026-10-20-dotnet11-aspnetcore-blazor-roth-video.md#busara-verdict-2026-10-20)).

- Akili: size **S**, a near-term GA-upgrade item that can start on Go Live RC2. Check
  `samples/04-telemetry/` for duplicated spans when moving over.
- Busara verdict: **Park**, fold into the .NET 11 GA upgrade. Re-timed 2026-10-20 by Steven to a
  near-term GA-upgrade item, since RC2 is Go Live.
- **Unparked by Steven on 2026-10-24** (was in timewarp-intel `parked.md`). It can be done now and
  aligned with the .NET 11 upgrade work (task 120, RC1 -> RC2 bump) rather than waiting for GA.

## Requirements

- `samples/04-telemetry` (server, wasm, apphost) relies on the framework's native ASP.NET Core /
  Blazor tracing where .NET 11 provides it. Today the server sample adds
  `tracing.AddAspNetCoreInstrumentation()` and references `OpenTelemetry.Instrumentation.AspNetCore`;
  remove or keep these based on what the native sources already emit.
- The Aspire dashboard shows no duplicated request / component spans for one action click.
- TimeWarp.State action spans (`TimeWarpStateTelemetry.ActivitySourceName`) still nest correctly
  under the framework spans, and the State devtools inspector still receives them (server and WASM).
- Devtools export (`source/timewarp-state-devtools-export`, `source/timewarp-state-devtools-wasm`)
  ignores or handles the new native framework spans without noise.
- Sample docs (`samples/04-telemetry/overview.md`) describe the native-tracing setup.

## Checklist

- [x] Inventory which spans .NET 11 emits natively (ASP.NET Core, Blazor Server, Blazor WASM) and their source names
- [x] Register the native sources in the sample's tracing setup; remove redundant manual instrumentation and package refs
- [x] Run the 04-telemetry AppHost and confirm no duplicated spans and correct nesting of State action spans
- [x] Confirm the devtools inspector still shows actions and state for server and WASM counters
- [x] Update `samples/04-telemetry/overview.md`
- [x] Build succeeds and tests green

## Session

- Created: 212639 (2026-10-24)
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session 01a183d6-7b24-75a3-a0e0-0c049af2e049, max-turns 500, Done — 2026-10-28T19:06:55Z
- Review oracle round 1: claude-opus-5-5 (ganda task work, headless), effort 1, roster general — 2026-10-29.
  Two bugs, one suggestion, and one nit are open. A fix pass was requested. See `review/round-1/merged.md`.
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session 76212403-ca61-4af0-a107-8fc0f0c68654, max-turns 80 — 2026-10-28T19:11:08Z
- Fix pass (implement-fix-2): addressed M1–M4. Committed the product diff, added the WASM framework-source test, reworded the package comment, and confirmed the inspector through `http://localhost:5086`.

## Results

The server sample subscribes to `Microsoft.AspNetCore`, `Microsoft.AspNetCore.Components`, `Microsoft.AspNetCore.Components.Server.Circuits`, and `Microsoft.AspNetCore.SignalR.Server`. It no longer references `OpenTelemetry.Instrumentation.AspNetCore`. That package enriches the same HTTP activity, so it adds a dependency and no spans. DevTools export, on the server and in WebAssembly, still copies only `TimeWarp.State`.

One click on `/counter` through the AppHost proxy `http://localhost:5086` produced one trace (`37360c3dbe7accdbbf33b2ce347217f3`): depth 2, total spans 2. The parent is source `Microsoft.AspNetCore.Components`, name `Event onclick -> Sample04Server.Components.Pages.Counter.IncrementCount` (span `a906d6503fa693ff`, 11.01 ms). Its only child is `CounterState.IncrementCountActionSet.Action` on `TimeWarp.State` (span `41d2075497001417`, parent span id `a906d6503fa693ff`, 4.18 ms).

The same proxy origin is the DevTools CORS origin. `/counter-wasm` discovered `http://localhost:35025`, posted `POST /ingest/spans` with status 200, and the inspector showed a live **wasm** row `IncrementCountActionSet.Action` / `WasmCounterState` (count 8) beside the server **srv** row. No CORS failure.

`IgnoreAFrameworkComponentActivity` starts a `Microsoft.AspNetCore.Components` activity that carries State tags and asserts the inspector log never receives it. The State activity in the same run is posted.

`./bin/dev test` exited 0: analyzer 44 passed, source generator 86 passed, state 131 passed and 1 skipped, plus 40 passed and 1 skipped, telemetry 21 passed, devtools 103 passed, devtools isolation 4 passed, client integration 64 passed and 1 skipped, architecture 7 passed and 1 skipped.

### How to validate

Smoke: `./bin/dev test`, then `aspire run --detach --non-interactive --apphost samples/04-telemetry/apphost/sample-04-apphost.csproj`. Open the sample endpoint the dashboard lists (the launch-profile proxy, `http://localhost:5086` on this run). Click **Click me** once on `/counter` and once on `/counter-wasm`. Open the **TimeWarp DevTools** endpoint from the dashboard.

Expect: `./bin/dev test` exits 0, including `DevToolsWasmExporter_Should.IgnoreAFrameworkComponentActivity`. The `/counter` trace has two spans: the component event span and one child `CounterState.IncrementCountActionSet.Action`. The inspector shows that server action with runtime **srv** and a `WasmCounterState` action with runtime **wasm**. The sample project does not reference `OpenTelemetry.Instrumentation.AspNetCore`.

## Review

- Round 1: Blocked — open M1, M2, M3, M4
- Fix pass: M1, M2, M3, and M4 are implemented on this branch. Round 2 has not reviewed them.

## Notes

- Related: task 120 (bump .NET 11 RC1 -> RC2). Doing 120 first gives the RC2 framework spans; not a
  hard dependency.
- Do not change TimeWarp.State's own action-span shape; this task is about the framework spans
  around it.
