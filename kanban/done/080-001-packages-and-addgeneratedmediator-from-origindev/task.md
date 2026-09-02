# Packages and AddGeneratedMediator from origin/dev

## Description

Parent: **080**. Swap martinothamar packages for **TimeWarp.Mediator 14.0.0-beta.1** and register the **generated** mediator. Do this on the **origin/dev** line, not origin-home `master`.

Wait for mediator **005-003** (package on nuget.org) unless the operator waives to a 14.0.0-beta.1 project reference after **005-001**.

## Requirements

- Task branch includes `origin/dev` (net10 + current martinothamar State). Do not implement on `origin/master`.
- Remove `Mediator.Abstractions` / `Mediator.SourceGenerator`
- Add TimeWarp.Mediator 14-beta packages (Contracts + Generators; Analyzers as needed)
- `AddGeneratedMediator()` (unscoped) for this slice; scopes are **080-002**
- `[assembly: MediatorAssembly]` (or equivalent membership) so handlers are linked
- Pipeline behaviors that today register as open generics must be `[assembly: MediatorBehavior]` (or the generated equivalent)
- `AddMediator()` (legacy reflection) must **not** be the State path

## Checklist

- [x] Branch from origin/dev (origin/dev is an ancestor of origin/master; task branch includes both)
- [x] Package refs
- [x] Generated registration + membership
- [x] Solution builds (handlers discovered)

## Out of scope

- Client vs server `ISender<TScope>` (080-002)
- E2E soak (080-003)

## Session

- Created: 154892 (2026-09-01)
- 2026-09-03: implementer (Claude Fable, ganda task work) claimed; TimeWarp.Mediator 14.0.0-beta.1 confirmed on nuget.org (mediator 005-003 done), no project-reference waiver needed.
- 2026-09-03: review oracle (Claude Fable, ganda task work) ran tw-implementation-review, effort 1 (general reviewer: Claude Opus subagent; fixes: Claude Sonnet subagent). Artifacts under `review/`.

## Results

TimeWarp.State, TimeWarp.State.Plus and TimeWarp.State.Policies now build against **TimeWarp.Mediator 14.0.0-beta.1** (Contracts + Analyzers on the libraries; Generators on every host). The martinothamar `Mediator.Abstractions` / `Mediator.SourceGenerator` packages are gone from `Directory.Packages.props`. `AddMediator()` is not called anywhere; every host calls the generated `AddGeneratedMediator()`.

### Library (source/)

- `[assembly: MediatorAssembly]` on TimeWarp.State and TimeWarp.State.Plus (`assembly-marker.cs`), so the host generator links Commit/Start (ReduxDevTools), the Plus ActionSet handlers, `LoadPersistentStateRequestHandler` and `StateInitializedNotificationHandler`. Verified in the test-app's generated `MediatorServiceCollectionExtensions.g.cs`.
- State pipeline behaviors are compile-time: `[assembly: MediatorBehavior(typeof(ReduxDevToolsBehavior<,>), order: 100)]`, `StateInitializationPreProcessor` 200, `StateTransactionBehavior` 300, `RenderSubscriptionsPostProcessor` 400 (same order as the old DI registration). Hosts declare their own behaviors with `order >= 500`. `MessagePreProcessor`/`MessagePostProcessor` subclasses were rewritten as plain `IPipelineBehavior` (TimeWarp shape: `Task<TResponse> Handle(request, RequestHandlerDelegate<TResponse> next, ct)`; the delegate parameter must be named `next` because the generated code passes it by name).
- Runtime opt-ins survive the compile-time weave: `ReduxDevToolsBehavior` takes its interop/options/store as optional dependencies and passes through when `UseReduxDevTools` was not called; `StateTransactionBehavior` reads `TimeWarpStateOptions.UseStateTransactionBehavior`; `PersistentStatePostProcessor` (Plus, host opt-in) treats the Blazored storage services as optional and logs `PersistentStatePostProcessor_StorageNotRegistered` instead of failing the action.
- `TimeWarp.State.IAction` is deleted; `TimeWarp.Mediator.IAction` is the marker (State analyzer TW0001 now checks `TimeWarp.Mediator.IAction`). `TimeWarp.State.ActionHandler<T>` is renamed **`StateActionHandler<T>(IStore)`** deriving from `TimeWarp.Mediator.ActionHandler<T>` (`ValueTask Handle`), because `ActionHandler<T>` in both namespaces is ambiguous for every consumer that imports both. Void handlers return `Task`, notification handlers return `Task`, `ISender.Send`/`IPublisher.Publish` return `Task` (no `.AsTask()`).
- `RenderSubscriptionsPostProcessor` is now public: the host's generated code references closed behavior types by name.
- Manual `IRequestHandler`/`IPipelineBehavior<,>` DI registrations removed from `AddTimeWarpState`/`UseReduxDevTools`; `LogTimeWarpStateMiddleware` lists the generator's closed behavior registrations.
- Policies: `CreateActionHandlerPolicy` targets `TimeWarp.Mediator.ActionHandler<>`. The default overload keeps the "public sealed Handler" rule (library handlers such as TimeWarp.State.Plus are referenced by the host's generated code); `CreateActionHandlerPolicy(requirePublicHandlers: false, ...)` relaxes it to "sealed Handler" for app assemblies whose handlers resolve inside the host and may stay internal (test-app-architecture-tests uses this).

### Hosts (tests/, samples/)

- test-app-client, the 5 sample hosts and `client-integration-tests` call `AddGeneratedMediator()`; app behaviors moved from `AddScoped(typeof(IPipelineBehavior<,>), ...)` to `mediator-behaviors.cs` (`order: 500..540`, same order as before).
- Every concrete request/action type (and its containing state classes in the samples) is `public`: the generator emits public `Send(TRequest)`/`Dispatch_*` methods, so internal requests are CS0051 (there is no `GenerateTypesAsInternal` equivalent). **Mediator feedback for 080-003.**
- Analyzer tests reference `TimeWarp.Mediator.Contracts.dll` instead of `Mediator.dll`.

### Known gaps / follow-ups

- Public API break (IAction namespace, `StateActionHandler`, `ValueTask`/`Task` shapes): TimeWarp.State needs a major bump before release (not done here; release is out of scope).
- `scripts/test.cs` no longer compiles against the floating `TimeWarp.Amuru` package (`ExecuteAsync` missing) — pre-existing, unrelated; the five suites were run individually.
- `ganda repo audit` reports pre-existing repo items (Nuru outdated, tools/dev-cli missing, journal gitignore patterns, `<Version>` in source/Directory.Build.props) — untouched.
- Scoped `ISender<ClientPipeline>`/`ServerPipeline` is 080-002; docs/readme and 049–051 disposition are 080-003.
- `tests/test-app/test-app-client/generated/**` is gitignored but 99 files are tracked (pre-existing on origin/dev), so builds churn the embedded worktree path. Review finding M6, accepted exception; 080-003 to untrack (or make emitted paths deterministic).
- Parallel `dotnet build --no-incremental` of the full solution can hit a pre-existing StaticWebAssets race in source/timewarp-state (wwwroot/js deleted before DefineStaticWebAssets); `git checkout -- source/timewarp-state/wwwroot/` restores. Not caused by this task.

### Review disposition

- Body: tw-implementation-review, effort 1, roster `general`; 2 rounds (round 2 = re-verification of fixes).
- Round 1: 0 bug, 6 suggestion, 4 nit. Round 2: all prior verified; 1 new nit.
- Final: 0 open; 5 suggestion fixed, 1 suggestion wontfix (M6, generated/ tracking, deferred to 080-003), 5 nit fixed.
- **Disposition: accepted-exceptions** (`review/disposition.md`).
- Fixes landed on this task in a0e0d707 (policy `requirePublicHandlers` overload, `order:` docs, PersistentStatePostProcessor trace guard, GetComponentOrder closed-type filter, constructor log names/event ids, integration-test comment, NoWarn 1591 on generator hosts, 066 warning on MultiTimerPostProcessor) plus the M11 comment reword. All five suites re-run green after the fixes; CS1591 count 0 (was 376).
- Paths: `review/review-framework.md`, `review/round-1/{general,merged}.md`, `review/round-2/{general,merged}.md`, `review/disposition.md`.

### How to validate

Smoke (from the task worktree; the local feed folder must exist):

```bash
mkdir -p artifacts/packages
# The samples restore TimeWarp.State 12.0.0-beta.3 from artifacts/packages; drop any cached copy from a
# pre-migration build so the NuGet cache cannot serve the stale assembly (no MediatorAssembly marker).
rm -rf ~/.nuget/packages/timewarp.state ~/.nuget/packages/timewarp.state.plus
dotnet build timewarp-state.slnx -c Debug
dotnet tool restore
dotnet fixie timewarp-state-analyzer-tests
dotnet fixie timewarp-state-tests
dotnet fixie timewarp-state-plus-tests
dotnet fixie client-integration-tests
dotnet fixie test-app-architecture-tests
grep -rn "Mediator.Abstractions\|Mediator.SourceGenerator\|AddMediator(" --include=*.csproj --include=*.props --include=*.cs source tests samples | grep -v /obj/ | grep -v /generated/
grep -c "GetRequiredService<global::TimeWarp" tests/test-app/test-app-client/generated/TimeWarp.Mediator.Generators/TimeWarp.Mediator.Generators.MediatorGenerator/MediatorServiceCollectionExtensions.g.cs
```

Expect:

- Solution build succeeds (source, tests, samples restore TimeWarp.Mediator 14.0.0-beta.1 from nuget.org and TimeWarp.State 12.0.0-beta.3 from artifacts/packages).
- Fixie: analyzer 10 passed; state 16 passed / 1 skipped; plus 11 passed / 1 skipped; integration 42 passed / 1 skipped; architecture 7 passed / 1 skipped; 0 failed.
- The grep for martinothamar packages / `AddMediator(` returns nothing.
- The generated registration file references State/Plus handlers (count > 0; 14 distinct handler types), and each IAction `Dispatch_*` method in `Mediator.g.cs` resolves `ReduxDevToolsBehavior`, `StateInitializationPreProcessor`, `StateTransactionBehavior`, `RenderSubscriptionsPostProcessor` in that order before the host's behaviors.
