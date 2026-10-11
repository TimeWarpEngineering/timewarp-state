# Development container validation fails without UseReduxDevTools because the generated mediator links CommitHandler

## Description

Surfaced by task 111 (PR #636). Once `samples/04-telemetry/server/sample-04-server` ran as Development
under Aspire (launch profile applied), ASP.NET Core's `ValidateOnBuild` container validation failed:
the TimeWarp.Mediator generator registers every handler it can see, including
`TimeWarp.State.Blazor`'s Redux DevTools `CommitHandler`, whose constructor requires the Redux
DevTools services (`IReduxDevToolsStore` and friends). Those services exist only after
`options.UseReduxDevTools()`. The sample now calls `UseReduxDevTools()` purely to satisfy validation
(`samples/04-telemetry/server/sample-04-server/program.cs`, comment explains) even though it never
renders `<ReduxDevTools />`.

So: any Development-mode Blazor Server app that references `TimeWarp.State.Blazor` and does not opt
into Redux DevTools fails at startup with a container validation error. That is a product defect in
the package, not a sample problem. It is the same dead path the Fable review flagged (task 109 M1,
"Redux DevTools time travel unreachable"; task 104 remove-reflection / finish-or-delete).

Filed by Claude (cockpit) at Steven's request, 2026-10-12.

## Requirements

- A Blazor app that calls `AddTimeWarpState()` + `AddTimeWarpStateBlazor()` without
  `UseReduxDevTools()` passes `ValidateOnBuild` / `ValidateScopes` in Development. Pick one:
  - register the Redux DevTools handlers only inside `UseReduxDevTools()` (preferred if the generator
    can exclude them, e.g. an attribute or a separate assembly/package for the feature), or
  - make `CommitHandler` (and any sibling handlers) tolerate missing services (optional dependencies,
    no-op when Redux DevTools is not enabled), or
  - finish task 104 / 109 M1's "finish or delete" decision and delete the feature.
  Steven decides the direction; record it in `## Notes` before implementing.
- Remove the `UseReduxDevTools()` workaround from sample 04 once the package no longer needs it.
- Regression test: build a host with `AddTimeWarpState().AddTimeWarpStateBlazor()` and
  `ValidateOnBuild = true`, `ValidateScopes = true`; `BuildServiceProvider` must not throw. Put it with
  the architecture tests (`tests/timewarp-state-tests/architecture/add-timewarp-state-blazor-tests.cs`).
- Docs: `documentation/topics/` note on what `UseReduxDevTools()` registers and that it is optional.

## Checklist

- [ ] Direction chosen and recorded (exclude from generator / optional deps / delete)
- [ ] Package change so validation passes without `UseReduxDevTools()`
- [ ] Regression test with ValidateOnBuild + ValidateScopes
- [ ] Sample 04 workaround removed; AppHost still runs clean on Linux
- [ ] Docs updated
- [ ] `ganda repo audit` clean; build warning-free for touched projects; tests green

## Session

- Created: cockpit (2026-10-12)

## Notes

- Related: 104, 109 (M1), 111 (PR #636, where the workaround was added), 103 (DevTools app).
- Repro: set `ASPNETCORE_ENVIRONMENT=Development` on any Blazor Server host using
  `TimeWarp.State.Blazor` 12.0.0-beta.11 without `UseReduxDevTools()`; or revert the
  `options.UseReduxDevTools()` lines in sample 04's `program.cs` and run the sample 04 AppHost.
