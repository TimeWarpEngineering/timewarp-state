# Round 1 — general
**Date:** 2026-09-03
**Scope reviewed:** commit 9d05efa5 vs origin/dev (excluding kanban/)

## Summary

The swap from martinothamar `Mediator` to TimeWarp.Mediator 14.0.0-beta.1 is coherent and, as far as I could verify, behaviour-preserving. I re-derived the old pipeline order (`UseReduxDevTools` runs inside `configureTimeWarpStateOptionsAction`, i.e. before the three `AddScoped(typeof(IPipelineBehavior<,>), …)` calls in `AddTimeWarpState`), and the new compile-time orders 100/200/300/400 match it exactly; the generated `Mediator.g.cs` dispatch chains confirm `ReduxDevTools → StateInitialization → StateTransaction → RenderSubscriptions → host 500..540 → handler`. Each rewritten behaviour is semantically equivalent to the `MessagePreProcessor`/`MessagePostProcessor` it replaced (pre-work then `next`; `next` then post-work; same try/catch/finally and rethrow semantics), the runtime opt-ins are correctly honoured (MS DI does bind constructor default values when a service is unregistered, so the `= null` optional dependencies work), and I verified the Results' claims by building (0 errors) and running all five suites: analyzer 10, state 16/1 skipped, plus 11/1, integration 42/1, architecture 7/1 — exactly as stated. The grep claim that no `Mediator.Abstractions`/`Mediator.SourceGenerator`/`AddMediator(` reference survives outside `kanban/` also holds.

No correctness defects found. The remaining findings are guard/documentation/hygiene regressions: an architecture rule that stopped enforcing the one thing it now depends on, three XML docs advertising an attribute syntax that will not compile, an inaccurate comment about what the integration-test host now weaves in, an unbumped local-feed package version for an incompatible assembly change, and ~376 new CS1591 warnings.

## Issues

### Issue 1 — Severity: suggestion
- File: source/timewarp-state-policies/policies.action-handler-policy.cs:30
- Description: The handler policy dropped `.BePublic()` and now only asserts `.BeSealed()`. The rule's own message states that "handlers shipped in a referenced library (e.g. TimeWarp.State.Plus) must be public so the host's generated code can reference them" — but that requirement is no longer enforced anywhere. `tests/timewarp-state-plus-tests/architecture-tests.cs:16` runs this exact policy against the **TimeWarp.State.Plus library assembly**, which is precisely the case where public is mandatory. If someone adds an `internal sealed class Handler` to Plus, this architecture test still passes and the failure surfaces instead as a `CS0122` inside every consuming host's generated `MediatorServiceCollectionExtensions.g.cs` — a much worse diagnostic, and one that only appears downstream.
- Suggestion: Keep `BePublic()` for library assemblies — e.g. add an optional `bool requirePublicHandlers = true` parameter (or a separate `CreateLibraryActionHandlerPolicy`) and have `test-app-architecture-tests` opt out while `timewarp-state-plus-tests` keeps the stricter rule. Note the sibling `CreateActionPolicy` (policies.action-policy.cs:61) still enforces `BePublic()` for actions, so the two rules are now asymmetric.
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs:7
- Description: Three new doc comments tell hosts to opt in with `[assembly: MediatorBehavior(typeof(X<,>), Order = ...)]`. `MediatorBehaviorAttribute.Order` is a **get-only** property (decompiled from `TimeWarp.Mediator.Contracts` 14.0.0-beta.1: `public int Order { get; }`, set only by the `MediatorBehaviorAttribute(Type behaviorType, int order = 0)` constructor). Copying the documented snippet yields `CS0617: 'Order' is not a valid named attribute argument`. The repo's own working call sites all use the positional `order:` form (`source/timewarp-state/assembly-marker.cs:13-16`, `tests/test-app/test-app-client/mediator-behaviors.cs:8-12`), so the docs contradict the code. Same defect at `source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs:5` and `source/timewarp-state-plus/assembly-marker.cs:6`.
- Suggestion: Replace `Order = ...` with `order: ...` in all three places. (`source/timewarp-state/assembly-marker.cs:11` prose "Hosts declare their own behaviors with Order >= 500" is fine as prose but reads better as `order`.)
- Status: open

### Issue 3 — Severity: suggestion
- File: tests/client-integration-tests/infrastructure/testing-convention.cs:29
- Description: The comment says the generated mediator weaves in "the ActiveActionBehavior pipeline behavior". It actually weaves in all five behaviours declared by `tests/test-app/test-app-client/mediator-behaviors.cs`: `PrePipelineNotificationRequestPreProcessor` (500), `PostPipelineNotificationRequestPostProcessor` (510), `PersistentStatePostProcessor` (520), `ActiveActionBehavior` (530) and `EventStreamBehavior` (540) — verified in the generated `Dispatch_*` chains. Before this commit the integration host registered *only* `ActiveActionBehavior` (old line 38), so the test pipeline silently widened. Two consequences worth being explicit about: (a) `EventStreamBehavior` now issues two extra `AddEventActionSet.Action` dispatches per action in the integration host; (b) `PersistentStatePostProcessor` now runs there but `AddBlazoredSessionStorage()`/`AddBlazoredLocalStorage()` are not registered in this convention, so every `[PersistentState]` action serializes its state, logs `PersistentStatePostProcessor_StorageNotRegistered` and skips the save. That is exactly the scenario the new optional-dependency handling was written for, but it means the integration suite exercises a persistence pipeline that never persists.
- Suggestion: Correct the comment to list all five woven behaviours, and either register the Blazored storage services here (so persistence is actually covered) or state explicitly that persistence is intentionally inert in this host.
- Status: open

### Issue 4 — Severity: suggestion
- File: Directory.Build.props:5
- Description: `TimeWarpStateVersion` stays at `12.0.0-beta.3` even though this commit changes the packaged `TimeWarp.State`/`TimeWarp.State.Plus` assemblies incompatibly (`TimeWarp.State.IAction` deleted, `ActionHandler<T>` → `StateActionHandler<T>`, `ValueTask<Unit>` → `ValueTask`/`Task`, new `[assembly: MediatorAssembly]`/`[assembly: MediatorBehavior]` metadata that hosts now depend on). All six sample projects consume these as **packages** from the `artifacts/packages` local feed, so a machine that already has `~/.nuget/packages/timewarp.state/12.0.0-beta.3` extracted from a pre-migration build will silently restore the old assembly: no `MediatorAssembly` marker, no woven behaviours, and confusing `CS0246`/`CS0122` errors in the samples. This is the exact failure documented in `kanban/done/076-migrate-samples-to-martinothamarmediator.md` ("the cached copy … was a stale intermediate … caused the earlier CS0122/CS0051 errors"). The Results acknowledge a major bump is needed "before release", but the hazard is present now, for every sample build.
- Suggestion: Bump `TimeWarpStateVersion` to a fresh prerelease (e.g. `13.0.0-beta.1`) as part of this change so the local feed and the NuGet cache cannot disagree, or document the `dotnet nuget locals` / cache-clear step in the task's validation section.
- Status: open

### Issue 5 — Severity: suggestion
- File: Directory.Build.props:29
- Description: The build now emits **376 CS1591 warnings** (measured: `dotnet build timewarp-state.slnx -c Debug --no-incremental` → 376 CS1591 + 4 pre-existing NU1510, 0 errors), all of them from `TimeWarp.Mediator.Generators` output (`Mediator.g.cs`, `MediatorServiceCollectionExtensions.g.cs`, `MediatorManifest.g.cs`) in the six sample hosts and the test app. Under martinothamar these types were suppressed by `options.GenerateTypesAsInternal = true`; the generated TimeWarp.Mediator surface is public and there is no equivalent switch, while `GenerateDocumentationFile` is `true` repo-wide (line 30) and `1591` is not in `NoWarn` (line 29). `TreatWarningsAsErrors` is `false` so nothing breaks today, but the signal-to-noise of the build log is now poor and any future warnings-as-errors gate would fail on generated code.
- Suggestion: Add `1591` to the repo-wide `NoWarn`, or set `<NoWarn>$(NoWarn);1591</NoWarn>` on the host projects that pull in the generator (the samples and `test-app-client`) — `client-integration-tests` already does this.
- Status: open

### Issue 6 — Severity: suggestion
- File: .gitignore:315
- Description: `/tests/test-app/test-app-client/generated/**` is gitignored, yet 99 files under that directory are tracked (tracked files win over `.gitignore`). This commit therefore had to commit build output: `_imports_razor.g.cs` was rewritten purely to swap the embedded absolute path `/…/timewarp-state/dev/…` for `/…/timewarp-state/task-080-001-packages-and-addgeneratedmediator-from-origindev/…` (44 changed lines of pure path churn), and the two `*_Persistence.g.cs` files were refreshed from a state that was already stale relative to `origin/dev` (they still contained a `LoadActionSet` that the current persistence generator no longer emits — and the generator source is not touched by this commit). Every contributor's build will rewrite these files with their own worktree path, guaranteeing recurring spurious diffs and merge conflicts.
- Suggestion: `git rm --cached -r tests/test-app/test-app-client/generated` so the existing ignore rule takes effect (the files are regenerated on build), or, if some of them are deliberately snapshot-tested, un-ignore only those and set `<EmitCompilerGeneratedFiles>`/`DeterministicSourcePaths` so the emitted content is path-independent.
- Status: open

### Issue 7 — Severity: nit
- File: source/timewarp-state/extensions/service-collection-extensions.log-timewarp-state-middleware.cs:22
- Description: `GetComponentOrder` is `public` API and its selection semantics changed silently: it used to match descriptors whose `ServiceType` **is** the open generic `componentType`, and now matches any descriptor whose `ImplementationType` is generic and **implements** `componentType`. The new form produces the right answer for the generated closed registrations (verified: `Distinct()` over the per-request registration order yields ReduxDevTools → StateInitialization → StateTransaction → RenderSubscriptions → the five host behaviours). But it also still matches a legacy `AddScoped(typeof(IPipelineBehavior<,>), typeof(X<,>))` registration, which under the generated mediator is completely inert — so a user migrating from the old model would see their behaviour listed in the "Pipeline Behavior Registrations" log while it never actually runs. That is precisely the diagnostic this helper exists to prevent.
- Suggestion: Restrict the scan to closed constructed types (`implementationType.IsConstructedGenericType`), so open-generic DI registrations are excluded from the log; optionally log a warning when any open-generic `IPipelineBehavior<,>` registration is present, since it now has no effect.
- Status: open

### Issue 8 — Severity: nit
- File: source/timewarp-state-plus/features/persistence/pipeline/persistent-state-post-processor.cs:63
- Description: The `LogTrace` calls at lines 63-69 and 78-84 evaluate `JsonSerializer.Serialize(state)` eagerly (arguments are evaluated before `ILogger.Log` checks the level) and, in the new code, they run *before* the `SessionStorageService is null` / `LocalSessionStorageService is null` checks that skip the save. Now that the behaviour is woven into every host of an assembly that declares it — including test hosts with no storage registered, see Issue 3 — every `[PersistentState]` action pays a full state serialization for a save that is then abandoned.
- Suggestion: Move the null check above the `LogTrace`, and/or guard the serialization with `if (Logger.IsEnabled(LogLevel.Trace))`.
- Status: open

### Issue 9 — Severity: nit
- File: source/timewarp-state/features/pipeline/state-transaction-behavior.cs:40
- Description: `StateTransactionBehavior`'s constructor logs `typeof(ReduxDevToolsBehavior<,>).GetSimpleName()`, so its "constructing {ClassName}<…>" message names the wrong behaviour; symmetrically `source/timewarp-state/features/redux-dev-tools/redux-dev-tools-behavior.cs:41` logs under `EventIds.StateTransactionBehavior_Constructing`. Both are pre-existing cross-wirings, but both constructors were rewritten in this commit and the two behaviours are now always constructed for every action (they used to be conditionally registered), so the misleading pair now appears in every startup log.
- Suggestion: Use `typeof(StateTransactionBehavior<,>).GetSimpleName()` here and add a `ReduxDevToolsBehavior_Constructing` event id for the other.
- Status: open

### Issue 10 — Severity: nit
- File: source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs:5
- Description: The new doc comment invites hosts to opt in ("the host declares `[assembly: MediatorBehavior(typeof(MultiTimerPostProcessor<,>), …)]`") without mentioning the known, still-open defect tracked in `kanban/to-do/066-add-internal-action-marker-and-fix-multitimerpostprocessor-recursion.md`: the behaviour is constrained to `TRequest : notnull` and unconditionally awaits `TimerState.ResetTimersOnActivity()`, which dispatches `ResetTimersOnActivityActionSet.Action` back through the same pipeline — unbounded recursion on the first action, with nothing breaking the cycle. Making the opt-in easier (a one-line assembly attribute) without a guard makes the trap easier to fall into. `source/timewarp-state-plus/features/timers/readme.md:48` also still documents the long-gone `AddScoped(typeof(IRequestPostProcessor<,>), …)` registration.
- Suggestion: Either add the recursion guard from task 066 (`if (request is ResetTimersOnActivityActionSet.Action) return response;`) or add an explicit "do not enable — see 066" warning to the doc comment; update the timers readme when 080-003 sweeps the docs.
- Status: open
