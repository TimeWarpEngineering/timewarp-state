# Task 128: Update to TimeWarp.Mediator 15.0.0-beta.1 (one package, ValueTask contracts, generated manifest)

## Description

TimeWarp.Mediator 15.0.0-beta.1 shipped on 2026-10-26 (TimeWarpEngineering/timewarp-mediator,
release `v15.0.0-beta.1`). It is a deliberate breaking redesign: the reflection runtime,
the `Task`-returning contracts, streams, the Aot/Link profiles and the separate Contracts and
Analyzers packages are gone. Two packages remain: `TimeWarp.Mediator` (contracts plus bundled
analyzer) and `TimeWarp.Mediator.Generators`. State is the first consumer dogfood; this task
gates 15.0.0 stable together with the Architecture and Nuru dogfoods.

State is currently on `14.0.0-beta.5`. Follow the State section of the migration page:
`documentation/migration.md` in timewarp-mediator at tag `v15.0.0-beta.1`
(https://github.com/TimeWarpEngineering/timewarp-mediator/blob/v15.0.0-beta.1/documentation/migration.md).
The contract page is `documentation/architecture.md` at the same tag, and the per-break list
is `documentation/release-notes/15.0.0.md`.

## Requirements

- `Directory.Packages.props`: replace the three pins (`TimeWarp.Mediator.Contracts`,
  `TimeWarp.Mediator.Generators`, `TimeWarp.Mediator.Analyzers` at `14.0.0-beta.5`) with
  `TimeWarp.Mediator` and `TimeWarp.Mediator.Generators` at `15.0.0-beta.1`. Every csproj
  that referenced Contracts or Analyzers references `TimeWarp.Mediator`. Only hosts keep
  `TimeWarp.Mediator.Generators`; a library that must not emit a mediator does not reference it.
- `ActionHandler<T>.Handle` returns `ValueTask`; `StateActionHandler<T>` keeps deriving from
  it. The `Task` shim is gone.
- These behaviors change from
  `Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)`
  to
  `ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)`
  and call `next(request, cancellationToken)`:
  `StateInitializationPreProcessor<,>`, `StateTransactionBehavior<,>`, `TelemetryBehavior<,>`,
  `RenderSubscriptionsPostProcessor<,>`, `ActiveActionBehavior<,>`, `MultiTimerPostProcessor<,>`,
  `PersistentStatePostProcessor<,>`.
- `LogTimeWarpStateMiddleware` and `PipelineBehaviorRegistry` read `MediatorManifest` (version 2:
  kind, scope, `Behaviors` in pipeline order; the `profile` key is gone). 15.0 does not
  register `IPipelineBehavior<,>` in the container, so an `IServiceCollection` scan for that
  interface is empty and must not be relied on.
- Hand-written `ISender` fakes drop both `CreateStream` methods; `Send` returns `ValueTask`.
- `Send` now throws synchronously for a null request, for `NoHandlerException`, and when a
  handler throws before its first await. A transient handler is one instance per scope.
  Adjust any test that awaited those as faulted tasks.
- Under `using static TimeWarp.Mediator.Unit`, signatures still bind to the System `ValueTask`;
  only a bare `ValueTask.X` in an expression is ambiguous (CS0229). Write `Unit.ValueTask` for
  the value.
- The action-set generator's `await Sender.Send(new X.Action(...))` stays. `IAction`,
  `ISender<ClientPipeline>`, `IPublisher<ClientPipeline>`, `Send(object)` for JavaScript
  dispatch and `AddGeneratedMediator<ClientPipeline>()` stay.
- `TimeWarpMediatorAssembly=false` is analyzer membership only; the generator never gated
  emission on it. Do not document or rely on it as an emission opt-out.
- Full repo build, tests, samples and the dev-cli workflow green with 0 warnings. Note any
  Mediator diagnostic ids (TWM001-TWM004, TWM007, TWM008, TWM010, TWM011) that fire and why.
- Record in `## Results` anything in the migration page that was wrong, missing or
  surprising for State. That feedback goes back to timewarp-mediator before 15.0.0 stable.

## Checklist

### Implementation
- [x] Package pins and csproj references updated to 15.0.0-beta.1
- [x] `ActionHandler<T>` and the seven behaviors on `ValueTask` / `MessageHandlerDelegate`
- [x] Middleware and registry read `MediatorManifest` v2
- [x] `ISender` fakes and sync-throw tests updated
- [ ] `dev workflow` (or the repo's equivalent full pipeline) green
### Documentation
- [x] State docs and agent.md mention `TimeWarp.Mediator` 15.0 (no Contracts/Analyzers ids)
- [x] `## Results` lists migration-page feedback for timewarp-mediator
### Review
- [ ] Code Review
- [ ] PR with merge commit

## Notes

- Source of truth for the break: timewarp-mediator task 021 and children 021-001..021-007
  (`documentation/design/2026-10-13-15.0-breaking-redesign-review.md` findings F1-F18).
- Architecture and Nuru have sibling dogfood tasks in their own repos. State goes first.
- The 15.0 benchmark gate measured the State pipeline shape at 131 ns / 0 B through the
  interface path (was 213 ns / 272 B on 14.0); if State has its own dispatch benchmark,
  re-record it.

## Goal (implementer restatement)

TimeWarp.State moves off TimeWarp.Mediator 14.0.0-beta.5 (three packages: Contracts, Generators, Analyzers) onto 15.0.0-beta.1. Contracts and the analyzer now ship as `TimeWarp.Mediator`. Only a host that emits a mediator references `TimeWarp.Mediator.Generators`. Handler and behavior signatures return `ValueTask` and call `next(request, cancellationToken)`. The pipeline log reads generated `MediatorManifest` version 2 instead of scanning the container for `IPipelineBehavior<,>`.

Acceptance:

- `Directory.Packages.props` pins `TimeWarp.Mediator` and `TimeWarp.Mediator.Generators` at `15.0.0-beta.1`. Every project that referenced Contracts or Analyzers references `TimeWarp.Mediator`. A library that must not emit a mediator does not reference Generators.
- `StateActionHandler<T>` still derives from `ActionHandler<T>`, whose `Handle` returns `ValueTask`. The seven named behaviors use `ValueTask<TResponse>` and `MessageHandlerDelegate<TRequest, TResponse>`.
- `LogTimeWarpStateMiddleware` and `PipelineBehaviorRegistry` read manifest version 2 (kind, scope, behaviors in pipeline order). They do not treat an `IPipelineBehavior<,>` service scan as the pipeline.
- Hand-written `ISender` fakes have no `CreateStream` methods, and `Send` returns `ValueTask`. Tests that expected a synchronous send failure to arrive as a faulted task match the synchronous throw.
- Living docs name TimeWarp.Mediator 15.0 and do not tell a reader to install Contracts or Analyzers.
- Build, tests, samples, and `dev workflow` are green with no warnings. Results name any TWM diagnostic that fires, and they list migration-page gaps for the mediator repo.

## Implementation Notes

- 

## Results

The 15.0.0-beta.1 pin, `ValueTask` handlers and behaviors, manifest v2 reader, and `ISender` fakes are in the tree. `./bin/dev workflow --mode pr` is green through the Fixie suites (including DevTools 101 and client-integration 64 passed, 1 skipped) and red at E2E. verify-samples did not run.

WebAssembly interactive mode throws on the first `Send`:

```
System.TypeLoadException: Recursive type definition detected System.Threading.Tasks.ValueTask`1
   at TimeWarp.Features.Routing.RouteState.PushRouteInfo(...)
   at TimeWarp.Features.Routing.TwPageTitle.OnAfterRenderAsync(Boolean firstRender)
```

The same exception is thrown from `Counter.ButtonClick` (`await Sender.Send(...)`). The counter stays at 3, persistence shows a new guid, and route changes do not stick. Interactive Server on CoreCLR increments 3 → 8. No TWM001–TWM011 diagnostics fired.

`TimeWarp.Mediator.Unit` is a struct whose static auto-property is `ValueTask<Unit> ValueTask { get; } = new(Instance)`. That emits a static field of type `ValueTask<Unit>` on `Unit`, and `ValueTask<T>` stores a `T`. Mono's WASM type loader treats that cycle as a recursive type. CoreCLR does not. State cannot move that field; every generated `Send` of an action constructs `ValueTask<Unit>`.

There is no State dispatch benchmark to re-record.

Pre-existing warnings the workflow does not fail on: NU1510, NU1902, NU1903, TW0007, CS8600, CS8602, RS0030, BL0010, BL0016, ASPDEPR011.

### Migration-page feedback for timewarp-mediator

- `Unit.ValueTask` as a static auto-property on the `Unit` struct breaks Blazor WebAssembly (`TypeLoadException: Recursive type definition detected System.Threading.Tasks.ValueTask`1`). A getter with no backing field, `public static ValueTask<Unit> ValueTask => new(Instance)`, or a field on a separate static class, keeps the name and removes the cycle. This blocks the State dogfood. https://github.com/TimeWarpEngineering/timewarp-mediator/blob/v15.0.0-beta.1/source/timewarp-mediator/unit.cs
- `AddGeneratedMediator` is emitted into `TimeWarp.Mediator.Generated` (or `TimeWarpMediatorNamespace`), and that namespace is not an implicit using. The State section says the call stays and does not say to import the namespace. That missing using was the compile break.
- `MediatorManifest` is internal. A library cannot read the type. State's public log takes the JSON string the host passes. A server host that references a client host cannot read the client's manifest, so one log call cannot list both pipelines.
- The manifest has no `profile` key. The reader rejects a version other than 2 and a present `profile` key.
- Analyzers do not flow transitively. `PrivateAssets=all` on the combined `TimeWarp.Mediator` package keeps it out of a packed nuspec and also drops the runtime DLL from `dotnet publish`. A published host (DevTools) must reference `TimeWarp.Mediator` without `PrivateAssets`. Replacing the old Analyzers package and keeping `PrivateAssets=all` hid the runtime assembly that used to flow from TimeWarp.State's public Contracts dependency.
- `IActionHandler` is gone. Easy to miss because the State analyzer still names that role.
- `ValueTask<T>` has no implicit conversion from `Task<T>`. `ContinueWith` is not on `ValueTask`. `Task.WhenAll` needs `.AsTask()`. The migration page mentions `Select`/`WhenAll` for `Send`, not behavior tests or `ContinueWith`.
- `TimeWarpMediatorAssembly=false` is analyzer membership only. The generator does not treat it as an emission opt-out.
- Dev-cli uses Nuru and `using static TimeWarp.Mediator.Unit`. Nuru was not on the migration's State list. State did not retarget Nuru.

### How to validate

Smoke: `./bin/dev workflow --mode pr`

Expect: exit 0, Fixie suites and Playwright e2e pass, verify-samples runs, no TWM diagnostic. Today the Fixie suites pass and E2E fails in WebAssembly with the `ValueTask<Unit>` type-load above. After a mediator build whose `Unit.ValueTask` has no static field of type `ValueTask<Unit>` on the `Unit` struct, re-pin and this smoke should go green.

## Blocked

2026-10-28: Blazor WebAssembly throws TypeLoadException: Recursive type definition detected System.Threading.Tasks.ValueTask`1 on the first ISender.Send. TimeWarp.Mediator 15.0.0-beta.1 Unit.ValueTask is a static auto-property, so the Unit struct holds a ValueTask<Unit> field and Mono's WASM loader rejects that cycle. Interactive Server and the Fixie suites pass. Unblocks when Unit.ValueTask has no ValueTask<Unit> field on the Unit struct (expression-bodied getter, or a field on another static class) and State re-pins that build. https://github.com/TimeWarpEngineering/timewarp-mediator/blob/v15.0.0-beta.1/source/timewarp-mediator/unit.cs

- https://github.com/TimeWarpEngineering/timewarp-mediator/blob/v15.0.0-beta.1/source/timewarp-mediator/unit.cs

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-26)
- Created: cockpit session 04be23b7 (2026-10-26) from the timewarp-mediator 15.0 train (task 021)
- 2026-10-28: implementer. Package, ValueTask, and manifest v2 landed. DevTools publish needed a direct `TimeWarp.Mediator` reference without `PrivateAssets` or the tools folder omitted the DLL. E2E is blocked on the WASM `ValueTask<Unit>` type load described above.
- 2026-10-28: `ganda repo audit --fix` scaffolded `glossary.md`, refreshed `.githooks/pre-push.cs`, and pinned TimeWarp.Nuru to 4.0.0-beta.7 (dev-cli still compiles). Advisory leftovers: task 098 has no worktree (not this id); kebab-path warnings are Blazor `*.lib.module.js` names, a `.targets` file, and backlog task 110's folder.
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session 01a18176-fe3a-71e1-b97e-beb2de02ad39, max-turns 500, Blocked — 2026-10-28T08:56:32Z
- 2026-10-28: cockpit. Unblocked. timewarp-mediator task 024 merged (PR #117) and `TimeWarp.Mediator` / `TimeWarp.Mediator.Generators` 15.0.0-beta.2 are on nuget.org: `Unit.ValueTask` is now an expression-bodied getter with no backing field, so the Mono WASM `TypeLoadException` is gone. Re-pin both packages to 15.0.0-beta.2, re-run the e2e lane and verify-samples, then finish the task.
