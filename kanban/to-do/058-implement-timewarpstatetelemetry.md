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
- Cache per-closed-generic reflection in `static readonly` fields (review finding 19).
- `AddTimeWarpStateTelemetry()` service-collection extension; use `TryAdd*` (review finding 23).
- Works on Blazor Server out of the box; document WASM via Aspire dashboard browser telemetry (OTLP/HTTP + CORS — auto-configured when the AppHost launches both app and dashboard; standalone dashboard needs `DASHBOARD__OTLP__CORS__ALLOWEDORIGINS`). WASM uses the JS OTel SDK, so it is not zero-JS, but it is standard maintained SDK code instead of the custom `redux-dev-tools.ts`/`timewarp-state.ts` layer.
- Sample or test-app wiring demonstrating the action timeline in the Aspire dashboard.

## Checklist

- [ ] Decide snapshot/diff strategy (span events vs structured logs, sampling guard, diff-only after initial snapshot)
- [ ] Create project + csproj, add to timewarp-state.slnx
- [ ] ActivitySource + TelemetryBehavior implementation
- [ ] AddTimeWarpStateTelemetry registration extension
- [ ] Wire test-app or sample to Aspire dashboard and verify the action timeline
- [ ] Tests (behavior emits activity, status set on handler exception, zero cost when no listener attached)
- [ ] Package README, WASM/browser-telemetry notes
- [ ] Performance review (hot path: every action dispatch; must be near-zero cost with no listener)
- [ ] Security review (state payloads in telemetry may contain user data — document redaction/sampling)

## Notes

References:

- `code-review-2026-06-11.md` — findings 1, 10, 11, 18, 19, 23
- https://aspire.dev/dashboard/enable-browser-telemetry/
- https://github.com/dotnet/aspire/discussions/4575 (dashboard extensibility — no custom-page/plugin model yet)
- https://github.com/microsoft/aspire/discussions/10644 (Aspire roadmap 2025→2026)

Out of scope — follow-on tasks:

- Time-travel "control" channel: dev-only SignalR hub / minimal API invoking `Store.LoadStatesFromJson`/`Hydrate`. Prerequisite: fix the FullName key bug in `store.redux-dev-tools.cs` (review finding 1).
- Companion devtools Blazor app (dogfooding TimeWarp.State) registered as an Aspire resource via a hosting integration (`AddTimeWarpStateDevTools()`), linked from the dashboard; port into a dashboard page when Aspire's plugin model ships.
- Deprecation path for the existing ReduxDevTools JS-interop feature.
