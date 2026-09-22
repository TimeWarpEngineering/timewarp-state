# Implement TimeWarp.State.Telemetry

## Description

Create a new `TimeWarp.State.Telemetry` package that instruments the mediator pipeline with OpenTelemetry, providing the "observation" half of Redux DevTools (action log, state snapshots, diffs, timing) via standard OTel — viewable in the Aspire dashboard or any OTel backend (Jaeger, Seq, App Insights).

Motivation: the 2026-06-11 code review (`code-review-2026-06-11.md`) found the Redux DevTools integration is the most fragile layer in the library — time-travel throws under default options (FullName vs short-name key mismatch, finding 1), the JS `init` path never calls `DevTools.init()`, the Commit button is dead (finding 11), and the JS interop layer leaks a `DotNetObjectReference` per render with a startup race (finding 10). Replacing the browser-extension + custom JS transport with .NET-native telemetry eliminates that entire class of problems.

ReduxDevTools functionality splits into two halves that map to Aspire differently:

- **Observation** (action log, state snapshots, diffs) — telemetry is a near-perfect fit; the mediator pipeline is the ideal seam. Bonus over ReduxDevTools: trace correlation (an action that triggers an HTTP call shows as the parent span of that call).
- **Control** (time-travel, commit, import/export) — OTLP is one-directional (app → dashboard), so this needs a dev-only back-channel; explicitly out of scope here (see Notes).

## Requirements

- New project `source/timewarp-state-telemetry/` following existing package conventions (kebab-case files, Directory.Build.props inheritance, central package versions); add to `timewarp-state.slnx`.
- `ActivitySource` (e.g. `"TimeWarp.State"`) exposed so consumers can register it with `AddSource(...)` in their OTel setup.
- `TelemetryBehavior<TAction, TResponse>` pipeline behavior, registered alongside the existing behaviors (see `source/timewarp-state/features/pipeline/state-transaction-behavior.cs` for the pattern), emitting one `Activity` per dispatched action with:
  - action type name, state type, duration, success/failure status
  - state snapshot/diff carried as span events or structured logs, NOT large span attributes (payload size; consider diff-only after an initial full snapshot)
  - all serialization guarded behind listener/level checks and sampling — do not repeat the eager-serialization mistake found in `persistent-state-post-processor.cs` (review finding 18)
- Cache per-closed-generic reflection in `static readonly` fields (review finding 19). `typeof(TAction)` is fine. Do **not** cache `GetProperties()` and invoke later.

### AOT / trimming (required)

Observation-only telemetry must stay AOT-safe. Replacing Redux DevTools observation is a net win (no `Type.GetType`, no assembly-qualified rewrite, no custom JS bridge). The snapshot path is the hazard.

- Default span: action type name, state type name, duration, success/failure. **No payload.**
- Snapshots/diffs only when a listener is attached **and** the host opts in.
- Serialize only through **caller-supplied** `JsonSerializerOptions` / `JsonTypeInfo` (same options as `TimeWarpStateOptions` after 065). Never `new JsonSerializerOptions()` and `JsonSerializer.Serialize(object)` on open `TState`.
- Do not walk state properties with reflection to build a diff.
- Do not use `AssemblyQualifiedName` or `Type.GetType`.
- Register `TelemetryBehavior<TAction, TResponse>` the same way as `StateTransactionBehavior` (generated mediator) so closed types are visible to the trimmer.
- New csproj: `IsAotCompatible=true`. Trim-warn the sample. WASM browser telemetry stays the JS OTel SDK (document CORS); that JS is not the C# trimmer’s problem.
- Time-travel / `LoadStatesFromJson` stays out of scope (that is the AOT-hostile half).
- `AddTimeWarpStateTelemetry()` service-collection extension; use `TryAdd*` (review finding 23).
- Works on Blazor Server out of the box; document WASM via Aspire dashboard browser telemetry (OTLP/HTTP + CORS — auto-configured when the AppHost launches both app and dashboard; standalone dashboard needs `DASHBOARD__OTLP__CORS__ALLOWEDORIGINS`). WASM uses the JS OTel SDK, so it is not zero-JS, but it is standard maintained SDK code instead of the custom `redux-dev-tools.ts`/`timewarp-state.ts` layer.
- Sample or test-app wiring demonstrating the action timeline in the Aspire dashboard.

## Checklist

- [x] Decide snapshot/diff strategy (span events vs structured logs, sampling guard, diff-only after initial snapshot)
- [x] Create project + csproj, add to timewarp-state.slnx
- [x] ActivitySource + TelemetryBehavior implementation
- [x] AddTimeWarpStateTelemetry registration extension
- [x] Wire test-app or sample to Aspire dashboard and verify the action timeline
- [x] Tests (behavior emits activity, status set on handler exception, zero cost when no listener attached)
- [x] Package README, WASM/browser-telemetry notes
- [x] Performance review (hot path: every action dispatch; must be near-zero cost with no listener)
- [x] Security review (state payloads in telemetry may contain user data — document redaction/sampling)
- [x] AOT: default span has no payload; opt-in snapshots use caller `JsonTypeInfo` / `TimeWarpStateOptions`; `IsAotCompatible=true`; no `Type.GetType` / reflection property walk
- [x] Implementation review disposition (same task id)

## Notes

References:

- `code-review-2026-06-11.md` — findings 1, 10, 11, 18, 19, 23
- https://aspire.dev/dashboard/enable-browser-telemetry/
- https://github.com/dotnet/aspire/discussions/4575 (dashboard extensibility — no custom-page/plugin model yet)
- https://github.com/microsoft/aspire/discussions/10644 (Aspire roadmap 2025→2026)

Out of scope — follow-on tasks:

- Time-travel "control" channel: dev-only SignalR hub / minimal API invoking `Store.LoadStatesFromJson`/`Hydrate`. Prerequisite: fix the FullName key bug in `store.redux-dev-tools.cs` (review finding 1). That path is AOT-hostile; do not pull it into this package.

## Session

- Created: 2026-06 (code review)
- 2026-09-22: cockpit — AOT constraints added (default span has no body; caller JsonTypeInfo; IsAotCompatible). Not dispatched.
- Companion devtools Blazor app (dogfooding TimeWarp.State) registered as an Aspire resource via a hosting integration (`AddTimeWarpStateDevTools()`), linked from the dashboard; port into a dashboard page when Aspire's plugin model ships.
- Deprecation path for the existing ReduxDevTools JS-interop feature.
- Implementer: grok (2026-09-22) — package, tests, Blazor Server + Aspire AppHost sample, docs.
- Review: grok oracle 01a0c72e-6b6e-7491-a1ed-37db8f90dc33 (2026-09-22) — effort 1 general; rounds 1–2; disposition clean.

## Results

New packable `TimeWarp.State.Telemetry` instruments `ClientPipeline` with one OpenTelemetry `Activity` per dispatched `IAction`. Default spans are metadata-only (action type name, state type name, duration, Ok/Error). Opt-in snapshots are span events (`state.snapshot` then `state.diff`), serialized only through caller `JsonTypeInfo`, and skipped unless a listener is attached, the span is sampled (`IsAllDataRequested`), and `IncludeSnapshots` is true.

**Snapshot/diff strategy:** span events (not span attributes). First JSON for a state type in the scope is `state.snapshot`; later unequal JSON is `state.diff` (ordinal string compare, no `GetProperties` walk); equal JSON emits nothing. Guarded by `HasListeners` → `StartActivity` → `IsAllDataRequested`.

**Registration:** `[assembly: MediatorBehavior(typeof(TelemetryBehavior<,>), order: 350, Scope = typeof(ClientPipeline))]` — inside `StateTransactionBehavior` (300) and outside render (400) so handler failures are `Error` before the transaction swallows them. Duration is handler + render, not clone/Redux JS. `AddTimeWarpStateTelemetry()` `TryAdd`s options + scoped `StateSnapshotCache`.

**Sample:** `samples/04-telemetry/` Blazor Server counter + Aspire AppHost (`sample-04-apphost`). OTLP exporter is added only when `OTEL_EXPORTER_OTLP_ENDPOINT` is set (AppHost injects it).

**Not in this change:** time-travel / `LoadStatesFromJson`; `AddTimeWarpStateDevTools()` companion app; obsoleting `UseReduxDevTools` (docs point observation at telemetry).

### Files changed

- `source/timewarp-state-telemetry/` — package (`IsAotCompatible=true`)
- `tests/timewarp-state-telemetry-tests/` — Fixie tests (nested ActionSet names, Error inside swallowing transaction, order 350, truncate after compare)
- `samples/04-telemetry/` — server sample + AppHost
- `Directory.Packages.props`, `timewarp-state.slnx`, `scripts/test.cs`
- `documentation/topics/telemetry.md`, package/root READMEs, sample overview

### Key decisions

- ActivitySource name `TimeWarp.State` (no OpenTelemetry package dependency in the library; consumers `AddSource`)
- Order 350 so handler failures are Error inside the transaction; duration is handler + render
- Nested `DeclaringType` names: span `CounterState.IncrementCountActionSet.Action`, tag `IncrementCountActionSet.Action`
- Snapshots require `TypeInfoResolver.GetTypeInfo`; missing type info skips `GetState`
- `MaxSnapshotChars` (16_384) truncates event payload after full-JSON cache compare; not a substitute for redaction

### Test outcomes

`dotnet fixie timewarp-state-telemetry-tests` — all passed (emit activity, nested ActionSet names, error status + rethrow, Error when a transaction swallows, weave order 350, no `GetState` without listener or when unsampled, snapshot then diff via source-generated `JsonTypeInfo`, truncate after compare, skip when `IncludeSnapshots` is false or resolver returns null, `TryAdd` keeps first options).

Library build: 0 warnings. Sample Release build succeeds; trim warnings are Blazor Server framework (`AddRazorComponents` / `Router`), not this package. AppHost Release build: 0 warnings.

### How to validate

**Automated**

```bash
dotnet build tests/timewarp-state-telemetry-tests/timewarp-state-telemetry-tests.csproj -c Release
dotnet fixie timewarp-state-telemetry-tests
# expect: 13 passed
```

**Smoke**

```bash
./bin/dev pack
# expect: artifacts/packages/TimeWarp.State.Telemetry.12.0.0-beta.4.nupkg exists

dotnet build samples/04-telemetry/server/sample-04-server/sample-04-server.csproj -c Release
dotnet build samples/04-telemetry/apphost/sample-04-apphost.csproj -c Release
# expect: both succeed

dotnet run --project samples/04-telemetry/apphost/sample-04-apphost.csproj
# expect: Aspire dashboard URL printed; resource sample-04-server running
# open /counter, click Click me; Traces show span CounterState.IncrementCountActionSet.Action, source TimeWarp.State, status Ok
```

Standalone dashboard alternative: `aspire dashboard`, then `OTEL_EXPORTER_OTLP_ENDPOINT` + `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf` and `dotnet run --project samples/04-telemetry/server/sample-04-server/sample-04-server.csproj`.

**Expect**

- Default click: span name `CounterState.IncrementCountActionSet.Action`, tags `timewarp.state.action=IncrementCountActionSet.Action` and `timewarp.state.state_type=CounterState`, no `snapshot.json`
- Failed handler: `ActivityStatusCode.Error` and exception event (covered by tests)
- No listener: `GetState` is not called (covered by tests)

**Not in scope:** live time-travel control channel; WASM JS OTel SDK wiring (documented only).

### Review disposition

- Body: tw-implementation-review, effort 1, roster `general` (grok subagent, read-only); 2 rounds on branch `task/058-implement-timewarpstatetelemetry` vs `origin/master`.
- Round 1: 2 bug, 1 suggestion, 0 nit. M1 nested `ActionSet.Action` names; M2 Error status swallowed by `StateTransactionBehavior` at order 50; M3 truncate-before-compare. Fixed on this task id (`61884448`).
- Round 2: re-verified M1–M3; 0 new findings. `dotnet fixie timewarp-state-telemetry-tests` — 13 passed.
- Final: 0 open; 2 bug fixed; 1 suggestion fixed; 0 wontfix.
- **Disposition: clean** (`review/disposition.md`; framework `review/review-framework.md`; last ledger `review/round-2/merged.md`).

