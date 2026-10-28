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

- [ ] Inventory which spans .NET 11 emits natively (ASP.NET Core, Blazor Server, Blazor WASM) and their source names
- [ ] Register the native sources in the sample's tracing setup; remove redundant manual instrumentation and package refs
- [ ] Run the 04-telemetry AppHost and confirm no duplicated spans and correct nesting of State action spans
- [ ] Confirm the devtools inspector still shows actions and state for server and WASM counters
- [ ] Update `samples/04-telemetry/overview.md`
- [ ] Build succeeds and tests green

## Session

- Created: 212639 (2026-10-24)
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session 01a183d6-7b24-75a3-a0e0-0c049af2e049, max-turns 500, Done — 2026-10-28T19:06:55Z

## Notes

- Related: task 120 (bump .NET 11 RC1 -> RC2). Doing 120 first gives the RC2 framework spans; not a
  hard dependency.
- Do not change TimeWarp.State's own action-span shape; this task is about the framework spans
  around it.
