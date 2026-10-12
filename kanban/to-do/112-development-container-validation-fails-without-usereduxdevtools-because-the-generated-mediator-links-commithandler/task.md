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

## Decision

**Direction 3: delete the Redux DevTools feature.** Steven, 2026-10-12. The companion DevTools app
(tasks 103, 111, 113, 115) with its action log, state tree, and diff is the replacement; the Fable
review (109 M1) already called the browser-extension time-travel path unreachable, and task 104's
"finish or delete" question is answered here. This is a breaking change for 12.0.0-beta.11
(unreleased): `UseReduxDevTools()`, `<ReduxDevTools />`, and `IReduxDevToolsStore` go away.

## Requirements

- Remove the feature from `TimeWarp.State.Blazor`:
  `extensions/service-collection-extensions.use-redux-dev-tools.cs`,
  `features/components/ReduxDevTools.razor`, the whole `features/redux-dev-tools/` folder
  (`redux-dev-tools-behavior.cs`, `redux-dev-tools-interop.cs`, `redux-dev-tools-options.cs`,
  `dispatch-request.cs`, `redux-action.cs`, `components/timewarp-state-dev-component.cs`,
  `requests/commit/*`, `requests/start/*`, `requests/i-redux-request.cs`), and the TypeScript
  `wwwroot/typescript/redux-dev-tools.ts` + `redux-dev-tools-types.ts` with their built
  `wwwroot/js/*.js(.map)` and `wwwroot/types/*.d.ts`. Strip the redux branches from
  `TimeWarp.State.Blazor.lib.module.ts`, `constants.ts`, `timewarp-state.ts` and rebuild the JS
  (check `tsconfig.json` / the csproj TypeScript targets so the bundle is regenerated, not hand-edited).
  Update `component-trim-roots.cs`, `assembly-marker.cs`, `global-usings.cs`,
  `javascript-dispatch-registry.cs`, `add-timewarp-state-blazor.cs` (Design region mentions
  UseReduxDevTools ordering), and the package readme.
- Remove from `TimeWarp.State` core: `store/store.redux-dev-tools.cs` (this is the reflection-based
  `LoadStatesFromJson` / `Hydrate` lookup task 104 wanted gone), `IReduxDevToolsStore` in
  `store/i-store.cs`, the `Hydrate` surface on `state/i-state.cs` / `state/state.cs` if nothing else
  uses it, the `LoadStatesFromJson` event id in `event-ids.cs`, redux mentions in
  `extensions/timewarp-state-options.cs`, `assembly-marker.cs`, and the csproj (JS/content items).
  Do not remove `JsonRequestHandler` / JavaScript dispatch — that is the separate allow-listed
  interop feature (task from beta.8) and stays.
- Samples: delete `samples/01-redux-dev-tools/` (and its `images/`), renumbering nothing; drop
  `UseReduxDevTools()` / `<ReduxDevTools />` from samples 03, 04, 05, 06 and `samples/overview.md`;
  sample 04 loses the container-validation workaround that motivated this task.
- Test app: drop `UseReduxDevTools` / `<ReduxDevTools />` from `tests/test-app/test-app-client`
  (`program.cs`, `features/base/components/*`), `tests/test-app-server/components/Routes.razor`;
  regenerate the checked-in `generated/TimeWarp.Mediator.Generators/*` if they are committed.
  Delete `tests/timewarp-state-tests/redux-dev-tools/start-handler-tests.cs`; update
  `architecture/add-timewarp-state-blazor-tests.cs` and `javascript-interop/json-request-handler-dispatch-tests.cs`.
- **Regression test**: a host with `AddTimeWarpState()` + `AddTimeWarpStateBlazor()` builds a
  provider with `ValidateOnBuild = true` and `ValidateScopes = true` without throwing. Put it in
  `tests/timewarp-state-tests/architecture/add-timewarp-state-blazor-tests.cs`.
- Docs: delete `documentation/topics/add-redux-dev-tools.md` and its `toc.yml` entry; point
  `devtools.md`, `telemetry.md`, `enable-javascript-interop.md`, `overview.md`, `readme.md` and the
  telemetry/blazor package readmes at the companion DevTools app. Add the breaking change to
  `documentation/release-notes/release12.0.0-beta.11.md` and
  `documentation/migrations/migration12.0.0-beta.11.md` (replace `UseReduxDevTools()` +
  `<ReduxDevTools />` with `AddTimeWarpStateTelemetry()` + `AddTimeWarpStateDevToolsExporter()` and the
  Aspire `AddTimeWarpStateDevTools()`). Leave historical release notes and migrations alone; the
  2024 blip may stay as history.
- `ganda repo audit` clean, build warning-free for touched projects, full `./bin/dev test` green,
  `./bin/dev verify-samples` green (sample 01 removed from its list if it is enumerated), e2e green
  (test app no longer renders the component).
- Split: if the Blazor/JS deletion and the core-store deletion do not fit one reviewable PR, file a
  child (`ganda kanban reserve --parent 112`) for the docs/samples pass; the package deletion and the
  regression test ship first.

## Checklist

- [x] Blazor package: feature folder, component, extension, TypeScript + built JS, trim roots, registry, readme
- [x] Core: `store.redux-dev-tools.cs`, `IReduxDevToolsStore`, `Hydrate` surface (if unused), event id, options, csproj items
- [x] Samples: 01 deleted; 03/04/05/06 and overview cleaned; sample 04 workaround gone
- [x] Test app and tests cleaned; `start-handler-tests.cs` deleted; generated mediator files regenerated if committed
- [x] Regression test: ValidateOnBuild + ValidateScopes passes without `UseReduxDevTools`
- [x] Docs: topic deleted, toc, cross-links, release notes + migration for beta.11
- [x] `dev test`, `dev verify-samples`, e2e, `ganda repo audit` all green

## Session

- Created: cockpit (2026-10-12)
- Decision recorded: 2026-10-12 (Steven: delete; relayed by Claude cockpit)
- Implemented: 2026-10-12 (Grok). One change: package deletion, samples, docs, and the regression test. `IState<TState>` stays as the covariant self-type marker; only `Hydrate` is gone. `JsonRequestHandler` stays.
- Reviewed: 2026-10-12 (Claude Opus 5.5 review oracle, effort 3, roster general; general reviewer = Claude Sonnet subagent `a7388e257c7a9e941`).
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-12T08:17:45Z

## Results

`UseReduxDevTools()`, `<ReduxDevTools />`, `IReduxDevToolsStore`, and `State<T>.Hydrate` are removed from 12.0.0-beta.11. The generated mediator no longer links `CommitHandler`, so a Development host that calls `AddTimeWarpState()` and `AddTimeWarpStateBlazor()` passes `ValidateOnBuild` and `ValidateScopes` without a Redux registration. The action log, state tree, and diff are `AddTimeWarpStateTelemetry()` plus `AddTimeWarpStateDevToolsExporter()` and Aspire `AddTimeWarpStateDevTools()`. Sample 01 is deleted. Sample 04 no longer calls `UseReduxDevTools()` to satisfy container validation. No checked-in mediator generator output needed regenerating.

### How to validate

Smoke: `./bin/dev test` then `./bin/dev pack` and `./bin/dev verify-samples`, then `./bin/dev e2e`, then `ganda repo audit`.

Expect: `./bin/dev test` ends with "Tests completed successfully!". `verify-samples` ends with "Samples verified successfully!" and does not build `samples/01-redux-dev-tools`. `./bin/dev e2e` reports 12 passed. `ganda repo audit` exits 0. `AddTimeWarpStateBlazorTests.Should_.ValidateOnBuild_Without_ReduxDevTools` passes.

### Review disposition

- Effort 3, roster general. Rounds: 2 (round 1 general review, round 2 re-verify of the fix delta).
- Final counts: bug 0, suggestion 1 fixed, nit 2 wontfix, open 0.
- Disposition: **accepted-exceptions**. M1 fixed: the dead `ReduxDevToolsEnabled` build type and all its mappings are removed from `timewarp-state.slnx`. M2 wontfix: the "removed in beta.11" pointers in the package readmes are intended migration help. M3 wontfix: the test name is cited in Results and records the regression it guards.
- Re-verified after the fix: `dotnet build timewarp-state.slnx` gives 0 errors (Debug and Release), `./bin/dev test` is green, and `ganda repo audit` exits 0 (one advisory, pre-existing kebab-path warning on the Blazor `*.lib.module.*` names).
- Paths: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`.

## Notes

- Related: 104 (remove remaining reflection — the `Hydrate` reflection goes with this), 109 M1
  (Redux DevTools time travel unreachable), 111 (PR #636, where the sample 04 workaround was added),
  103 / 113 (companion DevTools app, the replacement), 058 (telemetry).
- Inventory taken 2026-10-12 with `grep -rlE "ReduxDevTools|IReduxDevToolsStore|redux-dev-tools|CommitHandler|LoadStatesFromJson"`
  over source, tests, samples, documentation; re-run it before declaring done.
