# Review framework — task 058

**Date:** 2026-09-22
**Host task:** kanban/in-progress/058-implement-timewarpstatetelemetry/
**Diff scope:** branch `task/058-implement-timewarpstatetelemetry` vs `origin/master` (product commit `3a59c4eb feat: add TimeWarp.State.Telemetry OpenTelemetry pipeline`)
**Plan / brief:** New packable `TimeWarp.State.Telemetry` instruments `ClientPipeline` with one OpenTelemetry `Activity` per dispatched `IAction`. Default spans are metadata-only. Opt-in snapshots are span events (`state.snapshot` then `state.diff`), serialized only through caller `JsonTypeInfo`, skipped unless a listener is attached, the span is sampled, and `IncludeSnapshots` is true. AOT-safe: no `Type.GetType`, no `AssemblyQualifiedName`, no `GetProperties` walk, `IsAotCompatible=true`. Sample `samples/04-telemetry/` Blazor Server + Aspire AppHost.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok review oracle 01a0c72e-6b6e-7491-a1ed-37db8f90dc33 (2026-09-22)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Product files in scope

- `source/timewarp-state-telemetry/` (new package)
- `tests/timewarp-state-telemetry-tests/` (new Fixie suite)
- `samples/04-telemetry/` (Blazor Server + AppHost)
- `Directory.Packages.props`, `timewarp-state.slnx`, `scripts/test.cs`
- `documentation/topics/telemetry.md`, `documentation/topics/add-redux-dev-tools.md`, `documentation/topics/toc.yml`
- `readme.md`, `samples/overview.md`

Surrounding call sites (not modified, still in review scope for regressions / pattern match):

- `source/timewarp-state/assembly-marker.cs` behavior orders (Redux 100, init 200, transaction 300, render 400)
- `source/timewarp-state/features/pipeline/state-transaction-behavior.cs` weave pattern
- `source/timewarp-state/extensions/type-extensions.cs` (`TryGetEnclosingStateType`)
- `source/timewarp-state/extensions/service-collection-extensions.add-timewarp-state.cs`

## Requirements to check

- New project kebab-case, Directory.Build.props inheritance, central package versions, slnx, `IsAotCompatible=true`
- ActivitySource name exposed for `AddSource` (task example `"TimeWarp.State"`)
- `TelemetryBehavior<TAction, TResponse>`: one Activity per dispatch; action type name, state type, duration, success/failure
- Snapshots/diffs as span events (not large span attributes); serialization guarded by listener/level/sampling
- Per-closed-generic reflection in `static readonly`; `typeof(TAction)` fine; do not cache `GetProperties()`
- Default span has no payload; snapshots only when listener attached **and** host opts in
- Serialize only through caller `JsonSerializerOptions` / `JsonTypeInfo`; never `new JsonSerializerOptions()` + `Serialize(object)` on open TState
- No property walk for diffs; no `AssemblyQualifiedName` or `Type.GetType`
- Register via `[assembly: MediatorBehavior]` same as `StateTransactionBehavior` (closed types visible to trimmer)
- `AddTimeWarpStateTelemetry()` uses `TryAdd*`
- Tests: emits activity, error status + rethrow, zero cost with no listener
- Sample or test-app wiring for Aspire dashboard action timeline
- Package README: WASM/browser telemetry, security (payloads may contain user data), performance (hot path)
- Time-travel / `LoadStatesFromJson` stays out of this package

## Round 2

Re-review after M1–M3 fixes:

- M1: nested `DeclaringType` display name for `XxxActionSet.Action` (span name + `timewarp.state.action`). Carry M1 as fixed or reopen. Test: `TelemetryTestState.IncrementCountActionSet.Action`.
- M2: weave order 350 (inside `StateTransactionBehavior` 300, outside render 400) so handler failures are Error. Carry M2 as fixed or reopen. Tests: swallow-outer still records Error; assembly attribute `order: 350`.
- M3: cache/compare full JSON; truncate event payload only; `snapshot.truncated` tag. Carry M3 as fixed or reopen.
- Scan the fix delta for new defects. Do not clobber round-1 files.
