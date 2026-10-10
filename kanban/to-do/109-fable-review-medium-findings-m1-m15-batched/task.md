# Task 109: Fable review Medium findings M1-M15 (batched)

## Description

Batch of the fifteen Medium findings from Claude Fable's full codebase review, task 102
(`kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md`, PR #629,
`00ade373`). One checklist item per finding. The High findings are separate tasks: 105 (H1), 106 (H2),
107 (H3), 108 (H4).

Filed 2026-10-10 at Steven's request (relayed by Amina). Not launched. This is a batch for tracking;
per the one-PR-per-task rule, split items into child tasks (`ganda kanban reserve --parent 109`) before
implementing rather than doing them all in one PR.

## Order

Fable's recommended order (verbatim, `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md`, Overall assessment):

> Recommended order of attack: the package dependency (one line), the transaction rollback guard (one `ReferenceEquals` check), a `[CloneShared]`-style attribute or equivalent for injected members, then the DevTools and persistence dead paths, then `IsAotCompatible` on the two remaining runtime packages.

Filed tasks in that order: 105 (H1, package dependency) -> 106 (H2, rollback guard) -> 107 (H3, share-by-reference attribute) -> 104 and 109 M1/M9 (dead DevTools and persistence paths) -> 109 M2 (AOT checks on Blazor and Plus). 108 (H4) and the remaining Medium items are not in Fable's ordered list; schedule them after 107.

## Checklist

- [ ] M1. Redux DevTools time travel unreachable / only trim-unsafe reflection: handled by task 104 (finish or delete, Steven decides); companion inspector is task 103
- [ ] M2. `IsAotCompatible` on `TimeWarp.State.Blazor` and `TimeWarp.State.Plus`, annotate/suppress with justification, `PublishTrimmed` smoke of `samples/05-persistence` in `dev verify-samples` (last in Fable's order)
- [ ] M3. Rethrow `OperationCanceledException` after rollback; consider an option to rethrow handler exceptions; document semantics
- [ ] M4. `Store.Reset` as `RemoveState` for every key under the per-type lock (or test-only, off `IStore`)
- [ ] M5. `IStore`: remove `GetSemaphore` after H2 (task 106), `WaitForInitializationAsync<TState>()`, `GetState(Type)` returns `IState`
- [ ] M6. `PushRouteInfo`: replace JavaScript `eval` for `document.title` with a module export or the title from `TwPageTitle`
- [ ] M7. Action tracking: send `CompleteProcessing` with `CancellationToken.None`; pre-cancelled token test
- [ ] M8. `TimerState`: `async Task` callback with try/catch logging; marshal through the circuit; consider `PeriodicTimer`/`TimeProvider`
- [ ] M9. Persistence: remove `PreRender`/`Server` until implemented, optional storage services in `PersistenceService`, add `AddTimeWarpStatePersistence()` (with M1, the "dead DevTools and persistence code" step in Fable's order)
- [ ] M10. Drop the "test" assembly-name sniff; require `StateTestOptions.Enable()` or make guarded members internal
- [ ] M11. Remove the Blazor Server `HttpClient` registration from `AddTimeWarpStateBlazor` (or explicit opt-in method)
- [ ] M12. `JsonRequestHandler`: delete or guard the `JsonSerializer.Serialize(JsonSerializerOptions)` debug log
- [ ] M13. Renumber diagnostics to `TWS0009`..`TWS0012`, prefix the three prose ids, symbol-based read-only rule, shipped/unshipped release files
- [ ] M14. Close test coverage gaps listed in the finding (incl. un-ignore persistence e2e, delete `sample-test.cs`, rename cloner test file, nuspec allow-list overlaps task 105)
- [ ] M15. One docs/readme/metadata pass for the drift listed in the finding (incl. duplicate event id 104)

## Acceptance criteria

- Every checklist item is either done (via a child task PR) or explicitly closed with a recorded reason
  (e.g. M1 resolved by task 104).
- Each child PR: build warning-free for touched projects, tests green, `ganda repo audit` clean.

## Findings (verbatim)

### M1 (see task 104)

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 57-62 (PR #629, merged as `00ade373`):

> ### M1. Redux DevTools time travel is unreachable from the shipped JavaScript, but keeps the core package's only trim-unsafe reflection
>
> - `source/timewarp-state-blazor/wwwroot/typescript/redux-dev-tools.ts:61` onward maps every `DISPATCH` payload (`COMMIT`, `JUMP_TO_STATE`, `IMPORT_STATE`, ...) to `undefined`, and `MessageHandler` ignores unmapped types. Only `START` reaches .NET. `CommitHandler` and `IReduxDevToolsStore.LoadStatesFromJson` are therefore dead at runtime.
> - `source/timewarp-state/store/store.redux-dev-tools.cs:43-71` carries seven `UnconditionalSuppressMessage` attributes (IL2026, IL2070, IL2072, IL2075, IL3050) to scan `AppDomain.CurrentDomain.GetAssemblies()` and invoke `Hydrate` through `MethodInfo.Invoke`. Together with `EnsureStates` (`service-collection-extensions.add-timewarp-state.cs:85`) these are the only reflection sites left after task 097, and 097's own requirement was "no reflection in TimeWarp.State at all, used or unused".
> - `State<T>.Hydrate` (`state/state.cs:62`) throws `NotImplementedException` by default; only five states in the repo override it.
> - Suggestion: pick one. Either finish time travel (map `JUMP_TO_STATE`/`JUMP_TO_ACTION`, have the clone generator emit `Hydrate` so the reflection goes away, and test it), or delete `LoadStatesFromJson`, `Hydrate`, `IState<TState>`, `CommitHandler` and the seven suppressions now. The scratch backlog already lists splitting DevTools into its own package; deleting is consistent with that.

### M2

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 64-69 (PR #629, merged as `00ade373`):

> ### M2. `IsAotCompatible` is set on two of four runtime packages; the reflection that trimmed WASM apps will hit is in the other two
>
> - `source/timewarp-state/timewarp-state.csproj` and `source/timewarp-state-telemetry/timewarp-state-telemetry.csproj` set `IsAotCompatible`. `source/timewarp-state-blazor/timewarp-state-blazor.csproj` and `source/timewarp-state-plus/timewarp-state-plus.csproj` do not, so no IL warnings are produced for them (the Release build log confirms zero IL warnings overall).
> - Reflection in those two packages: `TimeWarpStateComponent` parameter discovery via `GetProperties`/`GetMethod` (`check-complex-parameter-changed.cs`), `Expression.Compile` in `RegisterRenderTrigger` (`register-render-trigger.cs:153`), `JsonSerializer.Deserialize(string, Type, options)` and `Activator.CreateInstance` in `JsonRequestHandler` (`json-request-handler.cs:122`), reflection-based `JsonSerializer.Serialize(object, Type)` in `PersistentStatePostProcessor` and `PersistenceService`, and `GetCustomAttribute` in several Plus behaviors. Blazor WebAssembly publishes with trimming on by default, and the trimmer only warns about assemblies that opt into analysis.
> - No trimmed or AOT publish is in the CI pipeline (`tools/dev-cli/endpoints/workflow-command.cs`: build, test, e2e, pack, verify-samples). `samples/04-telemetry` has `EnableTrimAnalyzer` and the dev CLI itself is `PublishAot`, which is not the same thing.
> - Suggestion: set `IsAotCompatible` (or at least `IsTrimmable` + `EnableTrimAnalyzer`) on Blazor and Plus, annotate or suppress with justification, and add a `PublishTrimmed` smoke of `samples/05-persistence` to `dev verify-samples` with `TrimmerSingleWarn=false`.

### M3

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 71-75 (PR #629, merged as `00ade373`):

> ### M3. Transaction behavior swallows handler failures and cancellation, so callers cannot observe either
>
> - `state-transaction-behavior.cs:144` returns `default!` after rollback, for both exceptions and `OperationCanceledException`. `await counterState.IncrementCount(ct)` completes normally when the handler threw or when `ct` was cancelled. Cancellation is the worse case: cooperative cancellation contracts in .NET expect `OperationCanceledException` to propagate, and callers chaining actions will continue as if the first succeeded.
> - `ExceptionNotification` covers the error case for apps that subscribe; nothing covers cancellation.
> - Suggestion: rethrow `OperationCanceledException` after rollback (it is not a failure, as the Design region says, so publishing is right to skip, but swallowing is not). Consider a `TimeWarpStateOptions` switch to rethrow handler exceptions after publishing, for hosts that want `Send` to fault. Document the current semantics in `documentation/topics` either way; today they are only in a code comment.

### M4

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 77-80 (PR #629, merged as `00ade373`):

> ### M4. `Store.Reset` is a partial reset
>
> - `source/timewarp-state/store/store.cs:105` clears `States` only. `PreviousStates`, `Semaphores`, `StateInitializationLocks` and `StateInitializationTasks` keep their entries, no `CancelOperations` or `Dispose` runs, and a later `GetState` creates a new instance while `StateInitializationPreProcessor` may still await the old initialization task. `RemoveState` (`:74-100`) does cancel and dispose the semaphore, so the two paths disagree.
> - Suggestion: implement `Reset` as `RemoveState` for every key under the per-type lock, or document `Reset` as test-only and move it off `IStore`.

### M5

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 82-86 (PR #629, merged as `00ade373`):

> ### M5. `IStore` exposes implementation details: `GetSemaphore`, `StateInitializationTasks`, `GetState(Type)` returning `object`
>
> - `source/timewarp-state/store/i-store.cs:27` (`GetState(Type)` returns `object` although every implementation returns `IState`), `:29` (`GetSemaphore`), and `:37` (`StateInitializationTasks`). `GetSemaphore` returns null until the state exists, is used by exactly one caller (`route-state.push-route-info.cs:34`), and is no longer used by the transaction behavior. `StateInitializationTasks` is a mutable `ConcurrentDictionary<string, Task>` keyed by type full name, which `documentation/topics/persistence.md` tells users to await directly.
> - Consequence of the `GetSemaphore` null contract: when a host sets `UseStateTransactionBehavior = false`, nothing creates `RouteState` before the handler runs, `GetSemaphore` returns null and `PushRouteInfo` silently does nothing (`push-route-info.cs:35`).
> - Suggestion: remove `GetSemaphore` once H2 lands; replace the dictionary with `Task WaitForInitializationAsync<TState>()`; return `IState` from `GetState(Type)`. These are breaking changes that fit the 12.0 beta window.

### M6

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 88-91 (PR #629, merged as `00ade373`):

> ### M6. `PushRouteInfo` reads the page title with JavaScript `eval`
>
> - `source/timewarp-state-plus/features/routing/route-state/route-state.push-route-info.cs:41`: `JsRuntime.InvokeAsync<string>("eval", cancellationToken, "document.title")`. Any host with a Content Security Policy that omits `unsafe-eval` (the normal production setting) throws here, and the interop call is not guarded (BL0016 in the build log).
> - Suggestion: add a `getDocumentTitle` export to the Plus or Blazor script module (`download-file.js` shows Plus already ships JS), or pass the title from `TwPageTitle`, which already knows it.

### M7

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 93-96 (PR #629, merged as `00ade373`):

> ### M7. `ActiveActionBehavior` completes tracking with the request token, so a cancelled action leaves the busy indicator on
>
> - `source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs:72`: the `finally` sends `CompleteProcessing` with `cancellationToken`. If the token is already cancelled (which is exactly when `next` threw `OperationCanceledException`), the completion send is itself cancelled, the action stays in `ActiveActionList`, and `IsActive` is stuck true for the scope.
> - Suggestion: send the completion with `CancellationToken.None`, mirroring what `StateTransactionBehavior` does for `ExceptionNotification`. Add a test with a pre-cancelled token to `active-action-behavior-tests.cs`.

### M8

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 98-101 (PR #629, merged as `00ade373`):

> ### M8. `TimerState` publishes into a scoped pipeline from timer threads, through `async void`
>
> - `source/timewarp-state-plus/features/timers/timer-state/timer-state.cs:82,102`: `System.Timers.Timer.Elapsed` fires on a thread-pool thread (Blazor Server) and `OnTimerElapsed` is `async void`. An exception from any notification handler is unobserved and tears down the process; the publish runs outside the circuit's synchronization context and races with user actions on the same scoped `Store`. On WASM the timer runs on the single thread, so only the `async void` part applies there.
> - Suggestion: make the callback `async Task` wrapped in a try/catch that logs, and dispatch through the circuit (expose a hook, or document that `TimerElapsedNotification` handlers must marshal with `InvokeAsync`). Consider `PeriodicTimer` or `ITimer` from `TimeProvider` for testability.

### M9

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 103-108 (PR #629, merged as `00ade373`):

> ### M9. Persistence: two public enum values do nothing, and the service's dependencies disagree with the behavior's
>
> - `source/timewarp-state/features/persistence/persistent-state-method.cs` exposes `PreRender` and `Server`; `persistent-state-post-processor.cs:92` and `persistence-service.cs:107-108` are `TODO` no-ops for both. A `[PersistentState(PersistentStateMethod.Server)]` state compiles, passes the policies, and silently never persists.
> - `PersistenceService` (`persistence-service.cs:33`) requires both `ISessionStorageService` and `ILocalStorageService` through its constructor, while `PersistentStatePostProcessor` treats each as optional and logs when missing. A host that registers only `AddBlazoredLocalStorage` fails DI resolution of the load path with an unrelated-looking error.
> - `IPersistenceService` has no registration helper; `AddTimeWarpStateRouting` exists for routing but persistence requires the host to write `AddScoped<IPersistenceService, PersistenceService>()` plus an assembly attribute (readme and `documentation/topics/persistence.md`).
> - Suggestion: remove `PreRender` and `Server` until implemented (beta window), make the storage services optional in `PersistenceService` with the same warning, and add `AddTimeWarpStatePersistence()` that registers the service.

### M10

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 110-114 (PR #629, merged as `00ade373`):

> ### M10. The "test" assembly-name sniff is still the guard on public and internal members
>
> - `source/timewarp-state/state/state.cs:86`: `ThrowIfNotTestAssembly` passes any caller whose assembly full name contains "test" (ordinal, ignore case), unless `StateTestOptions.Enable()` was called. The Design region and `documentation/release-notes/release12.0.0-beta.5.md` say the sniff is removed "in the following release"; it is still present six betas later.
> - Guarded members include `ThemeState.Initialize(Theme)` (`theme-state.debug.cs:41`), which is public. An assembly named `Contoso.Latest` or `Acme.Testimonials` passes the check in production. `Assembly.GetCallingAssembly()` is also unreliable under JIT inlining without `[MethodImpl(MethodImplOptions.NoInlining)]`.
> - Suggestion: drop the sniff and require `StateTestOptions.Enable()`, or make the guarded `Initialize` overloads `internal` with `InternalsVisibleTo` for the test assemblies and delete the guard altogether.

### M11

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 116-119 (PR #629, merged as `00ade373`):

> ### M11. `AddTimeWarpStateBlazor` registers an `HttpClient` on Blazor Server
>
> - `source/timewarp-state-blazor/extensions/service-collection-extensions.add-timewarp-state-blazor.cs:47`: a scoped `new HttpClient { BaseAddress = NavigationManager.BaseUri }` is `TryAdd`ed when not running in the browser. A state library registering the app's `HttpClient` is a surprising side effect: it hides a missing host registration, bypasses `IHttpClientFactory`, and creates one `HttpClient` per circuit. The 12.0.0-beta.10 migration guide documents it, which at least makes it discoverable.
> - Suggestion: remove it in the beta window and document the one-line host registration, or move it to an explicit `AddTimeWarpStateServerHttpClient()`.

### M12

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 121-124 (PR #629, merged as `00ade373`):

> ### M12. `JsonRequestHandler` serializes `JsonSerializerOptions` itself on every scope construction
>
> - `source/timewarp-state-blazor/features/javascript-interop/json-request-handler.cs:50`: the constructor logs `JsonSerializer.Serialize(JsonSerializerOptions)` at Debug level, unconditionally. That reflects over the options object (including converter instances) on every circuit or WASM start, can throw for options that hold non-serializable converters, and the string is discarded when Debug logging is off.
> - Suggestion: delete the call, or guard with `Logger.IsEnabled(LogLevel.Debug)` and log a hand-picked subset of properties.

### M13

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 126-130 (PR #629, merged as `00ade373`):

> ### M13. Public diagnostic ids are inconsistent and three are not prefixed
>
> - `TWS001` (`state-implementation-analyzer.cs:17`) sits next to `TWS0001`..`TWS0008`; the difference is one zero and is easy to misconfigure in `.editorconfig`. `StateInheritanceTypeArgumentRule`, `StateSealedClassRule` and `StateReadOnlyPublicPropertiesRule` (`state-inheritance-analyzer.cs:21`, `state-read-only-public-properties-analyzer.cs`) are prose ids without a prefix, so they cannot be grouped or suppressed by family. All are listed as unshipped in `AnalyzerReleases.Unshipped.md`, so renaming now is cheap; after 12.0 it is a breaking change.
> - `StateReadOnlyPublicPropertiesAnalyzer` is syntax-only: it flags `internal set`, ignores `init`, and does not see positional record properties.
> - Suggestion: renumber to `TWS0009`..`TWS0012` before 12.0 GA, move the entries to `AnalyzerReleases.Shipped.md` at release, and make the read-only rule symbol-based.

### M14

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 132-138 (PR #629, merged as `00ade373`):

> ### M14. Test coverage gaps worth closing before 12.0
>
> - No test for interleaved actions on one state (H2), for `StateInitializationPreProcessor`, for `ReduxDevToolsBehavior`/`ReduxDevToolsInterop`, for `RouteState.ChangeRoute`, for `TimerState` elapsed and restart, for `TwPageTitle`/`TimeWarpPageRenderNotifier`, or for `JsonRequestHandler.InitAsync`/dispose lifecycle (task 063 fixed a leak there without a regression test).
> - `tests/test-app-end-to-end-tests/persistence-test-page-tests.cs:48` is `[Ignore]`d; the only browser proof of persistence is gone. `sample-test.cs` is a Playwright demo against playwright.dev and accounts for two permanent skips.
> - `tests/timewarp-state-tests/cloning/deep-cloner-tests.cs` is named and documented after the deleted `DeepCloner`; its Purpose region still says "AnyClone replacement".
> - No packaging test asserts package contents or dependencies (H1 would have been caught).
> - Suggestion: add the missing unit tests listed above, un-ignore the persistence e2e once Chromium installs in CI (it does on `ubuntu-latest`), delete `sample-test.cs`, rename the cloner test file, and add a nuspec allow-list check to `dev pack`.

### M15

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 140-148 (PR #629, merged as `00ade373`):

> ### M15. Docs, readmes and repo metadata have drifted from the code
>
> - `claude.md:127` says Nullable is disabled project-wide; `Directory.Build.props` enables it. `claude.md:55` references `./BuildAndPackageAnalyzer.ps1` and `claude.md:132` says lock files are enabled; neither file nor any `packages.lock.json` exists, and `./LocalNugetFeed` is now `artifacts/packages`.
> - `readme.md:2` badge points at `workflows/release-build.yml` (only `workflow.yml` exists); `readme.md:13` says dotnet 10.0 while the target is net11.0; `readme.md:37` uses a docfx `xref:` link that does not resolve on GitHub; the `Blazor-State` download badge is for the retired package id.
> - `source/timewarp-state-plus/readme.md:1` says dotnet 8.0, `:17` loads `Assets/Logo.svg` (the folder is `assets/logo.svg`, case-sensitive on GitHub), `:47-49` advertise `InputColor`, which lives in `tests/test-app/test-app-client/features/color/`, not in Plus. This readme is the NuGet package readme.
> - `documentation/partials/getting-started.md:8` links to `tree/master/Samples` (folder is `samples`).
> - `timewarp-state.slnx:69` lists `Build\documentation.yml`, which does not exist, and its solution folders are misnamed (`03-ServerSide` holds sample 00 server; `/source/` holds only the Blazor project while the other source projects are at root).
> - `source/timewarp-state/event-ids.cs:19,23` assign `104` to both `Store_SetState` and `LoadStatesFromJson`.
> - Suggestion: one docs pass with the list above; add `ai-context.md` as the single place that summarizes packages and point `claude.md` at it.

## Session

- Created: 2026-10-10 (Grok Bot, at Steven's request via Amina; not launched)

## Notes

- Related: 103 (companion DevTools app, relevant to M1), 104 (M1), 105 (H1; overlaps M14 packaging test),
  106 (H2; prerequisite for M5 `GetSemaphore` removal), 107 (H3), 108 (H4). M14 also lists the missing interleaved-actions test that 106 adds.
