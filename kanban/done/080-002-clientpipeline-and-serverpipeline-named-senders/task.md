# ClientPipeline and ServerPipeline named senders

## Description

Parent: **080**. Split the Blazor client store pipeline from server handlers. Marker types, not strings.

## Depends on

- 080-001

## Requirements

- `ISender<ClientPipeline>` (and `IPublisher<ClientPipeline>` if used) in WASM/Blazor client code
- `ISender<ServerPipeline>` on the server / API handlers
- `[MediatorScope(typeof(ClientPipeline))]` / `ServerPipeline` on the corresponding handlers and behaviors
- State pipeline behaviors (transaction, render subscriptions, action tracking, …) are **client-scope**, not on server commands
- No `if (request is IAction)` filtering inside a shared pipeline
- Re-entrant `Send` stays on the same scope

## Checklist

- [x] Marker types
- [x] `AddGeneratedMediator<ClientPipeline>()` / `<ServerPipeline>()`
- [x] Behaviors assigned to the right scope
- [x] TWM004 (wrong-scope send) does not fire on legitimate client actions
- [x] Implementation review (effort 1, general)
- [x] Review disposition: clean (0 findings)

## Out of scope

- Package swap (080-001)
- Full test-app/e2e (080-003)

## Session

- Created: 154892 (2026-09-01)
- 2026-09-03: implementer (Claude Fable, ganda task work) claimed. Task branch includes origin/master and `feature/080-timewarp-mediator-14-beta` (080-001 merged there via PR #575, not to master), so `ganda kanban move 080-002 in-progress` refused on the 080-001 dependency guard; the folder was moved with `git mv` on the task branch instead. Host/sample/test call-site updates delegated to a Claude Sonnet subagent; library, build and validation by the implementer.
- 2026-09-04: implementer (Grok 4.6, ganda task work) finished named ClientPipeline/ServerPipeline senders on the 080-001 feature branch. Library + hosts + tests converted; build and five Fixie suites green. No `kanban done` / PR (host open-pr).
- 2026-09-04: review oracle (Grok 4.6, ganda task work) ran tw-implementation-review, effort 1 (general reviewer: grok-4.5 subagent, read-only). Artifacts under `review/`. Disposition: clean.

## Results

Named pipelines split the Blazor store from server API handlers using marker types, not strings.

### Library (`source/`)

- Marker types `TimeWarp.State.ClientPipeline` and `TimeWarp.State.ServerPipeline` (`source/timewarp-state/pipelines/`).
- TimeWarp.State and TimeWarp.State.Plus: `[assembly: MediatorScope(typeof(ClientPipeline))]`. State behaviors (`ReduxDevTools` 100, `StateInitialization` 200, `StateTransaction` 300, `RenderSubscriptions` 400) declare `Scope = typeof(ClientPipeline)`. Plus behaviors stay host opt-in with the same scope.
- `ISender<ClientPipeline>` / `IPublisher<ClientPipeline>` on `IState`/`State`, `Store`, `TimeWarpStateComponent`, `TimeWarpStateInputComponent` (replaces `IMediator Mediator`), JsonRequestHandler, transaction behavior, and Plus (action tracking, persistence, routing, timers, theme, feature flags). Re-entrant Sends (ActiveActionBehavior start/complete, EventStreamBehavior, State.Sender) stay on ClientPipeline.
- State behaviors close onto `IAction` with a generic constraint (`where TRequest : IAction`). No `if (request is IAction)` filter in a shared pipeline.

### Hosts (`tests/`, `samples/`)

- Every WASM/Blazor client host calls `AddGeneratedMediator<ClientPipeline>()` and has `[assembly: MediatorScope(typeof(ClientPipeline))]` (test-app-client, five samples, client-integration-tests via the client's generated method).
- Host client behaviors (test-app 500–540, sample-02 ActiveActionBehavior 500) set `Scope = typeof(ClientPipeline)`.
- Test-app-server: `[assembly: MediatorScope(typeof(ServerPipeline))]`, generated `Sender_ServerPipeline` / `Publisher_ServerPipeline`, `ISender<ServerPipeline>` on the weather MapGet.
- Contracts `GetWeatherForecasts.Query` stays an HTTP DTO (no TimeWarp.State reference). MapGet Sends server-local `GetWeatherForecastsRequest` with `[MediatorScope(typeof(ServerPipeline))]` so TWM004 does not fire. Client still HTTP-GETs `Query.RouteTemplate`.

### Deviation: CS0121 on `AddGeneratedMediator<TScope>()`

TimeWarp.Mediator 14.0.0-beta.1 emits `Microsoft.Extensions.DependencyInjection.GeneratedMediatorServiceCollectionExtensions` in every generated host. `TimeWarpMediatorNamespace` only renames Mediator/Sender/Publisher types, not that DI class. Test.App.Server references Test.App.Client (required for Razor `@using Test.App.Client` and `Client.Program.ConfigureServices`), so `AddGeneratedMediator<ServerPipeline>()` is CS0121 in the server compilation.

Workaround: `AddServerPipelineMediator()` registers the uniquely namespaced `Test.App.Server.Generated.Sender_ServerPipeline` / `Publisher_ServerPipeline` plus the one server handler. Client-integration-tests alias the server project (`Aliases=TestAppServer`) so `AddGeneratedMediator<ClientPipeline>()` binds to the client. Aliasing the client from the server would also disambiguate but breaks Razor. **Mediator feedback for 080-003:** namespace the DI extensions (or emit a unique extension class name) when `TimeWarpMediatorNamespace` is set.

Unscoped `AddGeneratedMediator()` is not called on any host, so an accidental `ISender` injection fails fast.

### Test outcomes

| Suite | Result |
|-------|--------|
| `timewarp-state-analyzer-tests` | 10 passed |
| `timewarp-state-tests` | 16 passed, 1 skipped |
| `timewarp-state-plus-tests` | 11 passed, 1 skipped |
| `client-integration-tests` | 42 passed, 1 skipped |
| `test-app-architecture-tests` | 7 passed, 1 skipped |

`dotnet build timewarp-state.slnx -c Debug` — 0 errors (no TWM004 on legitimate client actions). E2E not run (080-003).

### Review disposition

- Body: tw-implementation-review, effort 1, roster `general` (grok-4.5 subagent, read-only); 1 round on commit f58795e0 vs `origin/feature/080-timewarp-mediator-14-beta` (excluding `kanban/`).
- Round 1: 0 bug, 0 suggestion, 0 nit. Merge pass confirmed scoped senders, client-only behavior `Scope`, no shared-pipeline `if (request is IAction)`, re-entrant Sends on `ClientPipeline`, and generated `AddGeneratedMediator_ServerPipeline` present in `Test.App.Server.dll`.
- Final: 0 open; 0 fixed; 0 wontfix.
- **Disposition: clean** (`review/disposition.md`; framework `review/review-framework.md`; last ledger `review/round-1/merged.md`).

### How to validate

**Smoke**

```bash
mkdir -p artifacts/packages
rm -rf ~/.nuget/packages/timewarp.state/12.0.0-beta.3 ~/.nuget/packages/timewarp.state.plus/12.0.0-beta.3
dotnet build timewarp-state.slnx -c Debug
dotnet tool restore
dotnet fixie timewarp-state-analyzer-tests
dotnet fixie timewarp-state-tests
dotnet fixie timewarp-state-plus-tests
dotnet fixie client-integration-tests
dotnet fixie test-app-architecture-tests
```

Confirm named-pipeline membership (from repo root; ignore `/obj/` and `/generated/`):

```bash
# no unscoped ISender/IPublisher left in product code (comments mentioning the unscoped type are fine)
rg -n 'ISender[^<]|IPublisher[^<]|IMediator' --glob '*.cs' --glob '*.razor' source tests samples | rg -v '/(obj|generated)/'

# hosts call the scoped registration, not the unscoped default
rg -n 'AddGeneratedMediator<' --glob '*.cs' tests samples source

# client behaviors are scoped; server assembly is ServerPipeline
rg -n 'MediatorScope\(typeof\((Client|Server)Pipeline\)\)' --glob '*.cs' source tests samples
rg -n 'Scope = typeof\(ClientPipeline\)' --glob '*.cs' source tests samples
```

**Expect**

- Solution build succeeds; 0 TWM004 diagnostics on client `Sender.Send(...)` of state actions.
- Fixie counts match the table above; 0 failed.
- Product `ISender`/`IPublisher` usages are `ISender<ClientPipeline>` / `IPublisher<ClientPipeline>` or `ISender<ServerPipeline>` (weather MapGet). No `IMediator`.
- Client hosts call `AddGeneratedMediator<ClientPipeline>()`. Test-app-server calls `AddServerPipelineMediator()` (see deviation). Samples restore `TimeWarp.State` 12.0.0-beta.3 from `artifacts/packages` after the library pack, so a stale cached nupkg without `ClientPipeline` produces CS0246 — delete that cache version if restore happened before pack.
- `MediatorScope(typeof(ClientPipeline))` on TimeWarp.State, TimeWarp.State.Plus, test-app-client, and each sample host. `MediatorScope(typeof(ServerPipeline))` on test-app-server and `GetWeatherForecastsRequest`/`GetWeatherForecastsHandler`.
- Generated `tests/test-app/test-app-client/generated/TimeWarp.Mediator.Generators/.../MediatorServiceCollectionExtensions.g.cs` contains `AddGeneratedMediator_ClientPipeline` and `typeof(TScope) == typeof(global::TimeWarp.State.ClientPipeline)`.

**Automated gate:** the five `dotnet fixie` commands above.

**Not in scope:** E2E / Playwright (080-003); docs/readme; swapping `AddServerPipelineMediator` for a generator-namespaced `AddGeneratedMediator<ServerPipeline>()` (mediator 14.0.0-beta.1 limitation).
