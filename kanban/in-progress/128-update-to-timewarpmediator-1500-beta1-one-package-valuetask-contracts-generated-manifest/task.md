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
- [ ] Package pins and csproj references updated to 15.0.0-beta.1
- [ ] `ActionHandler<T>` and the seven behaviors on `ValueTask` / `MessageHandlerDelegate`
- [ ] Middleware and registry read `MediatorManifest` v2
- [ ] `ISender` fakes and sync-throw tests updated
- [ ] `dev workflow` (or the repo's equivalent full pipeline) green
### Documentation
- [ ] State docs and agent.md mention `TimeWarp.Mediator` 15.0 (no Contracts/Analyzers ids)
- [ ] `## Results` lists migration-page feedback for timewarp-mediator
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

## Implementation Notes

- 

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-26)
- Created: cockpit session 04be23b7 (2026-10-26) from the timewarp-mediator 15.0 train (task 021)
